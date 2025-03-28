using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RessourceManager : NetworkBehaviour
{
    
    public static RessourceManager Instance { get; private set; }

    [SerializeField] private Vector2Int _ressourcesQuantityPerChunk = default;
    [SerializeField] private float _minDistanceBetweenResources = 10;

    [SerializeField] public Ressource_Library _ressourceLibrary = default;

    private Dictionary<Vector2Int, TileInfo> tileDictionary = new();
    private Dictionary<ulong, List<Vector2Int>> clientTileLookUp = new();

    private TerrainManager terrainManager;
    private BiomeGeneration biomeGeneration;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        terrainManager = FindAnyObjectByType<TerrainManager>();
        biomeGeneration = GetComponent<BiomeGeneration>();
        biomeGeneration.SetVariables(terrainManager, false);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    [ServerRpc(RequireOwnership = false)]
    public void LoadedCheckTileDictionnaryServerRpc(Vector2Int tilePosition, float[,] heights, ServerRpcParams rpcParams = default) //Probablement pas un string
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (tileDictionary.ContainsKey(tilePosition))
        {
            //Load ressources pour le joueur concerné avec un client rpc
            tileDictionary[tilePosition].clientIds.Add(clientId);
            clientTileLookUp[clientId].Add(tilePosition);
            ShowRessources(tilePosition, clientId);
        }
        else
        {
            //Genere des ressources pour cette tuile            
            //TileInfo startTile = new TileInfo(true);
            //tileDictionary.Add(tilePosition, startTile);
            GenerateRessources(tilePosition, heights, clientId);
        }        
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeloadedCheckTileDictionnaryServerRpc(Vector2Int tilePosition, ServerRpcParams rpcParams = default) //Probablement pas un string
    {
        
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (tileDictionary.ContainsKey(tilePosition))
        {
            tileDictionary[tilePosition].clientIds.Remove(clientId);
            clientTileLookUp[clientId].Remove(tilePosition);
            HideRessources( tilePosition, clientId);

            if (tileDictionary[tilePosition].clientIds.Count == 0)
            {
                UnloadTileRessources(tilePosition);
            }
        }
    }


    private void GenerateRessources(Vector2Int tilePosition, float[,] heights, ulong clientId) //Si aurait terrain, pourrait generer de 0 a n ressources. Apres les possent aleatoirement sur la tuile,
                                                  //et choisis le type exacte de ressource en fonction du biome a l'emplacement
    {
        List<ResourceData> ressources = new List<ResourceData>();
        int toGenerate = Random.Range(_ressourcesQuantityPerChunk.x, _ressourcesQuantityPerChunk.y);


        List<Vector2Int> generatedPositions = new List<Vector2Int>(); // Store previous positions

        for (int i = 0; i < toGenerate; i++)
        {
            Vector2Int newPoint;
            int attempts = 0;
            do
            {
                newPoint = new Vector2Int(
                    Random.Range(4, terrainManager.ChunkResolution - 4),
                    Random.Range(4, terrainManager.ChunkResolution - 4)
                );

                attempts++;
                if (attempts > 50) break; // Prevent infinite loops

            } while (generatedPositions.Any(p => Vector2Int.Distance(p, newPoint) < _minDistanceBetweenResources));

            generatedPositions.Add(newPoint);
            int xRes = newPoint.x;
            int yRes = newPoint.y;

            float xWidth = (float)terrainManager.ChunkWidth / (float)terrainManager.ChunkResolution * (float)xRes;
            float yWidth = (float)terrainManager.ChunkWidth / (float)terrainManager.ChunkResolution * (float)yRes;

            float continentalness = biomeGeneration.GenerateLayer(yRes, xRes, tilePosition.x, tilePosition.y, terrainManager.ContinentalnessBorderCurve, terrainManager.ContinentalnessNoiseScale, terrainManager.ContinentalnessNoiseOffsetX, terrainManager.ContinentalnessNoiseOffsetZ, terrainManager.ContinentalnessFrequency, terrainManager.ContinentalnessOctaves, terrainManager.ContinentalnessRoughness, terrainManager.ContinentalnessLacunarity, terrainManager.ContinentalnessGenerationPercentRange, terrainManager.ContinentalnessCutPercentRange, terrainManager.ContinentalnessIsReverse);
            float temperature = biomeGeneration.GenerateLayer(yRes, xRes, tilePosition.x, tilePosition.y, terrainManager.TemperatureBorderCurve, terrainManager.TemperatureNoiseScale, terrainManager.TemperatureNoiseOffsetX, terrainManager.TemperatureNoiseOffsetZ, terrainManager.TemperatureFrequency, terrainManager.TemperatureOctaves, terrainManager.TemperatureRoughness, terrainManager.TemperatureLacunarity, terrainManager.TemperatureGenerationPercentRange, terrainManager.TemperatureCutPercentRange, terrainManager.TemperatureIsReverse);
            float humidity = biomeGeneration.GenerateLayer(yRes, xRes, tilePosition.x, tilePosition.y, terrainManager.HumidityBorderCurve, terrainManager.HumidityNoiseScale, terrainManager.HumidityNoiseOffsetX, terrainManager.HumidityNoiseOffsetZ, terrainManager.HumidityFrequency, terrainManager.HumidityOctaves, terrainManager.HumidityRoughness, terrainManager.HumidityLacunarity, terrainManager.HumidityGenerationPercentRange, terrainManager.HumidityCutPercentRange, terrainManager.HumidityIsReverse);

            int currentBiome = biomeGeneration.ApplyBiome(continentalness, temperature, humidity);

            int width = terrainManager.ChunkResolution + 1;
            float height = (float)heights.GetValue(xRes * width + yRes) * terrainManager.ChunkHeight;
            //float height = biomeGeneration.GenerateHeight(y, x, continentalness, temperature, humidity) * terrainManager.ChunkHeight;

            Vector3 position = new Vector3(yWidth + tilePosition.x, height, xWidth + tilePosition.y);

            GameObject temp = _ressourceLibrary.GetResourceForBiome(currentBiome);
            if (temp != null)
            {
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject ressource = Instantiate(temp, position, randomRotation);
                ressource.GetComponent<NetworkObject>().Spawn();
                ressource.GetComponent<NetworkObject>().NetworkShow(clientId);

                ulong objectId = ressource.GetComponent<NetworkObject>().NetworkObjectId;
                ResourceData thisRessource = new("TEST", objectId, ressource.transform.position.x, ressource.transform.position.y, ressource.transform.position.z);
                ressources.Add(thisRessource);
            }         
        }
        AddTile(tilePosition, ressources, clientId);
        //Get de 0 a n comme chiffre aleatoire
        //Placer ces 0 a n points sur le terrain 
        //Get le biome au point 
        //Génere des ressources en funciton du biome du point.  

        //Puis ajoute la tuile au dictionnaire
        //Puis load pour le joueur clientId

    }

    private void ShowRessources(Vector2Int tilePosition, ulong clientId)
    {
        List<ResourceData> ressources = tileDictionary[tilePosition].resources;

        foreach (ResourceData ressource in ressources)
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ressource.ressourceId, out var spawnedObject))
            {
                spawnedObject.NetworkShow(clientId);
            }
        }
    }

    private void HideRessources(Vector2Int tilePosition, ulong clientId)
    {
        List<ResourceData> ressources = tileDictionary[tilePosition].resources;

        foreach (ResourceData ressource in ressources)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ressource.ressourceId, out var spawnedObject))
            {
                spawnedObject.NetworkHide(clientId);
            }
        }
    }

    private void UnloadTileRessources(Vector2Int tilePosition)
    {
        TileInfo thisTile = tileDictionary[tilePosition];       
        
        //UNLOAD OBJECTS HERE
        foreach (var ressource in thisTile.resources)
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ressource.ressourceId, out var spawnedObject))
            {                
                spawnedObject.Despawn();
            }            
        }
        tileDictionary.Remove(tilePosition);        
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDespawnRessourcesServerRpc(ulong objectId, int _idRessource, int _quantity, ulong _idClient, int _idPersonnage)
    {
        OpenWorld_QueryManager.Instance.PlayerGotRessource(_idRessource, _quantity, _idClient, _idPersonnage);
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var spawnedObject))
        {
            spawnedObject.Despawn();
        }
    }

    public void AddTile(Vector2Int tilePosition, List<ResourceData> ressources, ulong clientId)
    {
        //TileInfo tile = tileDictionary[tilePosition]; // Get a copy
        List<ulong> clients = new List<ulong>() { clientId };
        TileInfo tile = new TileInfo(ressources, clients);
        tileDictionary.Add(tilePosition, tile);
        clientTileLookUp[clientId].Add(tilePosition);        
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectServerRpc(ulong clientId)
    {
        Debug.Log("CONNECTED : " + clientId);
        List<Vector2Int> initialState = new List<Vector2Int>();
        clientTileLookUp.Add(clientId, initialState);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log("DISCONNECTED : " + clientId);
        foreach (Vector2Int tilePosition in clientTileLookUp[clientId])
        {
            tileDictionary[tilePosition].clientIds.Remove(clientId);
            clientTileLookUp.Remove(clientId);

            if (tileDictionary[tilePosition].clientIds.Count == 0)
            {                
                UnloadTileRessources(tilePosition);
            }
        }
    }
    
}

[System.Serializable]
public class ResourceData
{
    public string resourceName;
    public ulong ressourceId;
    //public int amount;
    float position_x;
    float position_y;
    float position_z;
    //float rotation;

    public ResourceData(string resourceName, ulong thisRessourceId, float x, float y, float z)
    {
        this.resourceName = resourceName;
        this.ressourceId = thisRessourceId;
        this.position_x = x;
        this.position_y = y;
        this.position_z = z;
    }
}

[System.Serializable]
public class TileInfo
{    
    public List<ResourceData> resources; // List of resources in this tile    
    public List<ulong> clientIds;  

    public TileInfo(List<ResourceData> ressources, List<ulong> thisClientId )
    {
        this.resources = ressources;      
        this.clientIds = thisClientId;        
    }
}