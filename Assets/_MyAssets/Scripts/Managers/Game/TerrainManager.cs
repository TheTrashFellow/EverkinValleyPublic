using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }
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

    #region |-- VARIABLES --|
    [Header("Map Settings")]
    [SerializeField] private Material _waterMaterial;
    [SerializeField] private int _seed = 42;
    [SerializeField] private float _oceanAltitude = 0.1f;
    [SerializeField] private Biome[] _biomes;
    [SerializeField] private GameObject _village;
    [SerializeField] private float _villageRate = 0.1f;
    [SerializeField] private int _villageMinimumDistance = 2;
    [Serializable]
    public class BiomeSetting
    {
        [SerializeField] private Vector2 _continentalness;
        [SerializeField] private Vector2 _temperature;
        [SerializeField] private Vector2 _humidity;

        public Vector2 Continentalness => _continentalness;
        public Vector2 Temperature => _temperature;
        public Vector2 Humidity => _humidity;
    }
    [Serializable] public class Biome
    {
        [SerializeField] private string _name;
        [SerializeField] private TerrainLayer _terrainTexture;
        [SerializeField] private BiomeSetting[] _biomeSettings;
        public TerrainLayer TerrainTexture => _terrainTexture;
        public BiomeSetting[] BiomeSettings => _biomeSettings;
    }
    public int Seed => _seed;
    public float OceanAltitude => _oceanAltitude;
    public Biome[] Biomes => _biomes;
    public GameObject Village => _village;
    public float VillageRate => _villageRate;
    public int VillageMinimumDistance => _villageMinimumDistance;

    [Header("Chunk Settings")]
    [SerializeField] private int _chunkWidth = 250;
    [SerializeField] private int _chunkHeight = 100;
    [SerializeField] private ChunkRes _chunkResolution = ChunkRes._128x128;
    [SerializeField] private ChunkRenderDist _chunkLoadingDistance = ChunkRenderDist._3x3;
    [SerializeField] private float _chunkLoadingDelay = 1f;
    [SerializeField] private enum ChunkRes
    {
        _16x16 = 16,
        _32x32 = 32,
        _64x64 = 64,
        _128x128 = 128,
        _256x256 = 256,
        _512x512 = 512,
        _1024x1024 = 1024,
        _2048x2048 = 2048,
        _4096x4096 = 4096
    }
    [SerializeField] private enum ChunkRenderDist
    {
        _3x3 = 1,
        _5x5 = 2,
        _7x7 = 3,
        _9x9 = 4,
        _11x11 = 5,
        _13x13 = 6,
        _15x15 = 7,
        _17x17 = 8,
        _19x19 = 9,
        _21x21 = 10,
        _23x23 = 11,
        _25x25 = 12,
    }
    public int ChunkWidth => _chunkWidth;
    public int ChunkHeight => _chunkHeight;
    public int ChunkResolution => (int)_chunkResolution;
    public int ChunkLoadingDistance => (int)_chunkLoadingDistance;

    [Header("Continentalness Settings")]
    [SerializeField] private float _continentalnessBorderCurve = 0.005f;
    [SerializeField] private float _continentalnessNoiseScale = 0.1f;
    [SerializeField] private float _continentalnessFrequency = 0.3f;
    [SerializeField] private int _continentalnessOctaves = 1;
    [SerializeField] private float _continentalnessRoughness = 0.5f;
    [SerializeField] private float _continentalnessLacunarity = 2f;
    [SerializeField] private Vector2 _continentalnessGenerationPercentRange = new(0, 1);
    [SerializeField] private Vector2 _continentalnessCutPercentRange = default;
    [SerializeField] private bool _continentalnessIsReverse = false;
    private float _continentalnessNoiseOffsetX;
    private float _continentalnessNoiseOffsetZ;
    public float ContinentalnessBorderCurve => _continentalnessBorderCurve;
    public float ContinentalnessNoiseScale => _continentalnessNoiseScale;
    public float ContinentalnessNoiseOffsetX => _continentalnessNoiseOffsetX;
    public float ContinentalnessNoiseOffsetZ => _continentalnessNoiseOffsetZ;
    public float ContinentalnessFrequency => _continentalnessFrequency;
    public int ContinentalnessOctaves => _continentalnessOctaves;
    public float ContinentalnessRoughness => _continentalnessRoughness;
    public float ContinentalnessLacunarity => _continentalnessLacunarity;
    public Vector2 ContinentalnessGenerationPercentRange => _continentalnessGenerationPercentRange;
    public Vector2 ContinentalnessCutPercentRange => _continentalnessCutPercentRange;
    public bool ContinentalnessIsReverse => _continentalnessIsReverse;

    [Header("Temperature Settings")]
    [SerializeField] private float _temperatureBorderCurve = 0.005f;
    [SerializeField] private float _temperatureNoiseScale = 0.1f;
    [SerializeField] private float _temperatureFrequency = 1f;
    [SerializeField] private int _temperatureOctaves = 1;
    [SerializeField] private float _temperatureRoughness = 0.5f;
    [SerializeField] private float _temperatureLacunarity = 2f;
    [SerializeField] private Vector2 _temperatureGenerationPercentRange = new(0, 1);
    [SerializeField] private Vector2 _temperatureCutPercentRange = default;
    [SerializeField] private bool _temperatureIsReverse = false;
    private float _temperatureNoiseOffsetX;
    private float _temperatureNoiseOffsetZ;
    public float TemperatureBorderCurve => _temperatureBorderCurve;
    public float TemperatureNoiseScale => _temperatureNoiseScale;
    public float TemperatureNoiseOffsetX => _temperatureNoiseOffsetX;
    public float TemperatureNoiseOffsetZ => _temperatureNoiseOffsetZ;
    public float TemperatureFrequency => _temperatureFrequency;
    public int TemperatureOctaves => _temperatureOctaves;
    public float TemperatureRoughness => _temperatureRoughness;
    public float TemperatureLacunarity => _temperatureLacunarity;
    public Vector2 TemperatureGenerationPercentRange => _temperatureGenerationPercentRange;
    public Vector2 TemperatureCutPercentRange => _temperatureCutPercentRange;
    public bool TemperatureIsReverse => _temperatureIsReverse;

    [Header("Humidity Settings")]
    [SerializeField] private float _humidityBorderCurve = 0.005f;
    [SerializeField] private float _humidityNoiseScale = 0.1f;
    [SerializeField] private float _humidityFrequency = 1f;
    [SerializeField] private int _humidityOctaves = 1;
    [SerializeField] private float _humidityRoughness = 0.5f;
    [SerializeField] private float _humidityLacunarity = 2f;
    [SerializeField] private Vector2 _humidityGenerationPercentRange = new(0, 1);
    [SerializeField] private Vector2 _humidityCutPercentRange = default;
    [SerializeField] private bool _humidityIsReverse = false;
    private float _humidityNoiseOffsetX;
    private float _humidityNoiseOffsetZ;
    public float HumidityBorderCurve => _humidityBorderCurve;
    public float HumidityNoiseScale => _humidityNoiseScale;
    public float HumidityNoiseOffsetX => _humidityNoiseOffsetX;
    public float HumidityNoiseOffsetZ => _humidityNoiseOffsetZ;
    public float HumidityFrequency => _humidityFrequency;
    public int HumidityOctaves => _humidityOctaves;
    public float HumidityRoughness => _humidityRoughness;
    public float HumidityLacunarity => _humidityLacunarity;
    public Vector2 HumidityGenerationPercentRange => _humidityGenerationPercentRange;
    public Vector2 HumidityCutPercentRange => _humidityCutPercentRange;
    public bool HumidityIsReverse => _humidityIsReverse;

    [Header("Height Settings")]
    [SerializeField] private Vector2 _heightWidth = default;
    [SerializeField] private float _interiorHeightMultiplicator = 1f;
    [SerializeField] private float _exteriorHeightMultiplicator = 1f;
    [SerializeField] private float _heightNoiseScale = 100f;
    [SerializeField] private float _heightFrequency = 0.01f;
    [SerializeField] private int _heightOctaves = 1;
    [SerializeField] private float _heightRoughness = 0.5f;
    [SerializeField] private float _heightLacunarity = 2f;
    [SerializeField] private Vector2 _heightGenerationPercentRange = default;
    [SerializeField] private Vector2 _heightCutPercentRange = default;
    [SerializeField] private bool _heightIsReverse = false;
    private float _heightNoiseOffsetX;
    private float _heightNoiseOffsetZ;
    public Vector2 HeightWidth => _heightWidth;
    public float InteriorHeightMultiplicator => _interiorHeightMultiplicator;
    public float ExteriorHeightMultiplicator => _exteriorHeightMultiplicator;
    public float HeightNoiseScale => _heightNoiseScale;
    public float HeightFrequency => _heightFrequency;
    public int HeightOctaves => _heightOctaves;
    public float HeightRoughness => _heightRoughness;
    public float HeightLacunarity => _heightLacunarity;
    public Vector2 HeightGenerationPercentRange => _heightGenerationPercentRange;
    public Vector2 HeightCutPercentRange => _heightCutPercentRange;
    public bool HeightIsReverse => _heightIsReverse;
    public float HeightNoiseOffsetX => _heightNoiseOffsetX;
    public float HeightNoiseOffsetZ => _heightNoiseOffsetZ;

    private List<GameObject> terrains = new();
    private Vector2 playerChunkPos;
    private bool chunkIsGenerating = false;
    private GameObject player;

    private RessourceManager ressourceManager;
    #endregion


    #region |-- UNITY METHODS --|
    private void Start()
    {
        InitiateVariables();
        ressourceManager = FindAnyObjectByType<RessourceManager>();
    }

    public bool isHalted = false;

    private void Update()
    {
        if (player != null && !isHalted)
        {
            GenerateChunks();
        }
    }

    private void OnDestroy()
    {
        foreach (var terrain in terrains)
        {
            Destroy(terrain);
        }
    }
    #endregion

    #region |-- CHUNKS GENERATION METHODS --|
    private void CreateStartChunks()
    {        
        for (int x = (int)GetPlayerChunkPos().x - ChunkLoadingDistance; x <= (int)GetPlayerChunkPos().x + ChunkLoadingDistance; x++)
        {
            for (int z = (int)GetPlayerChunkPos().y - ChunkLoadingDistance; z <= (int)GetPlayerChunkPos().y + ChunkLoadingDistance; z++)
            {
                CreateChunk(x, z, false);
                ServerSceneManager.Instance.FindVillageForChunkServerRpc(new((x * _chunkWidth) + (_chunkWidth / 2), (z * _chunkWidth) + (_chunkWidth / 2)), player.GetComponent<Player>().clientId);
            }
        }
    }

    public void CreateChunk(int chunkPosX, int chunkPosY, bool forceVillage)
    {
        TerrainData terrainData = new()
        {
            heightmapResolution = ChunkResolution + 1,
            baseMapResolution = ChunkResolution,
            alphamapResolution = ChunkResolution,
            size = new Vector3(_chunkWidth, _chunkHeight, _chunkWidth)
        };
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "Terrain";
        terrainObject.transform.position = new Vector3(chunkPosX * _chunkWidth, 0, chunkPosY * _chunkWidth);
        terrainObject.layer = LayerMask.NameToLayer("TerrainLayer");
        terrainObject.AddComponent<BiomeGeneration>().SetVariables(this, forceVillage);       
        terrains.Add(terrainObject);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(terrainObject.transform);
        plane.GetComponent<Renderer>().material = _waterMaterial;
        Destroy(plane.GetComponent<MeshCollider>());
        plane.transform.localPosition = new Vector3(_chunkWidth / 2, OceanAltitude, _chunkWidth / 2);
        plane.transform.localScale = new Vector3(0.1f * _chunkWidth, 1, 0.1f * _chunkWidth); // Unity's default Plane is 10x10 units

        plane.name = "Water";
    }

    public void GenerateTree(Vector2Int terrainPosition, float[,] heights)
    {
        ressourceManager.LoadedCheckTileDictionnaryServerRpc(terrainPosition, heights);
    }

    
    private void GenerateChunks()
    {
        if (GetPlayerChunkPos() != playerChunkPos)
        {
            List<Vector2> templist = new();

            for (int x = (int)GetPlayerChunkPos().x - ChunkLoadingDistance; x <= (int)GetPlayerChunkPos().x + ChunkLoadingDistance; x++)
            {
                for (int z = (int)GetPlayerChunkPos().y - ChunkLoadingDistance; z <= (int)GetPlayerChunkPos().y + ChunkLoadingDistance; z++)
                {
                    GameObject addTerrain = terrains.Find(terrain =>
                        terrain.transform.position.x == (x * _chunkWidth) &&
                        terrain.transform.position.z == (z * _chunkWidth)
                    );

                    if (addTerrain == null)
                    {
                        templist.Add(new(x, z));
                    }
                }
            }

            List<GameObject> deleteTerrain = terrains.FindAll(terrain =>
                terrain.transform.position.x < ((GetPlayerChunkPos().x - ChunkLoadingDistance) * _chunkWidth) ||
                terrain.transform.position.x > ((GetPlayerChunkPos().x + ChunkLoadingDistance) * _chunkWidth) ||
                terrain.transform.position.z < ((GetPlayerChunkPos().y - ChunkLoadingDistance) * _chunkWidth) ||
                terrain.transform.position.z > ((GetPlayerChunkPos().y + ChunkLoadingDistance) * _chunkWidth)
            );

            foreach (GameObject terrain in deleteTerrain)
            {
                ServerSceneManager.Instance.HideVillageToClientServerRpc(new(terrain.transform.position.x + (ChunkWidth / 2), terrain.transform.position.z + (ChunkWidth / 2)), player.GetComponent<Player>().clientId);
                DestroyTerrain(Vector2Int.RoundToInt(GetTerrainChunkPos(terrain)));
            }

            if (!chunkIsGenerating)
            {
                playerChunkPos = GetPlayerChunkPos();
                StartCoroutine(MoveChunks(templist));
            }
        }
    }

    private void DestroyTerrain(Vector2Int terrain)
    {
        try
        {
            GameObject terrainObject = default;
            Ray ray = new Ray(new((terrain.x * ChunkWidth) + (ChunkWidth / 2), 50f, (terrain.y * ChunkWidth) + (ChunkWidth / 2)), Vector3.down); // Example: downward ray
            RaycastHit hit;

            int layerMask = LayerMask.GetMask("TerrainLayer"); // Replace with your layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                terrainObject = hit.collider.gameObject;
            }

            Destroy(terrainObject);
            terrains.Remove(terrainObject);
            ressourceManager.DeloadedCheckTileDictionnaryServerRpc(new Vector2Int((int)terrainObject.transform.position.x, (int)terrainObject.transform.position.z));
        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void RefreshChunk(Vector2Int terrain)
    {
        DestroyTerrain(terrain);
        CreateChunk(terrain.x, terrain.y, true);
    }

    IEnumerator MoveChunks(List<Vector2> templist)
    {
        while (templist.Count > 0)
        {
            chunkIsGenerating = true;
            for (int i = templist.Count - 1; i >= 0; i--)
            {
                Vector2 temp = templist[i];
                CreateChunk((int)temp.x, (int)temp.y, false);
                ServerSceneManager.Instance.FindVillageForChunkServerRpc(new(((int)temp.x * _chunkWidth) + (_chunkWidth / 2), ((int)temp.y * _chunkWidth) + (_chunkWidth / 2)), player.GetComponent<Player>().clientId);
                templist.RemoveAt(i);
                yield return new WaitForSeconds(_chunkLoadingDelay);
            }
            chunkIsGenerating = false;
            yield return null;
        }   
    }

    public void AssignPlayer(GameObject _player, Vector3 position)
    {
        player = _player;
        if (player != null)
        {
            Debug.LogError("In terrain Manager" + position);
            if(position == new Vector3(0,0,0))
            {
                float playerY = (player.transform.localScale.y / 2) + 30 + (_chunkHeight * 0);
                player.transform.position = new Vector3(player.transform.position.x, playerY, player.transform.position.z);
            }
            else if (position.y < 0)
            {
                position.y = 30;
                player.transform.position = position;
            }
            else
            {
                player.transform.position = position;
            }

            CreateStartChunks();
        }
    }
    #endregion

    #region |-- GET METHODS --|
    private Vector2 GetPlayerChunkPos()
    {
        if (player != null)
        {
            return new(Mathf.Floor(player.transform.position.x / _chunkWidth), Mathf.Floor(player.transform.position.z / _chunkWidth));
        }
        else return default;
    }

    public Vector2 GetTerrainChunkPos(GameObject terrain)
    {
        return new(terrain.transform.position.x / _chunkWidth, terrain.transform.position.z / _chunkWidth);
    }
    #endregion

    #region |-- OTHERS --|
    private void InitiateVariables()
    {
        UnityEngine.Random.InitState(_seed);
        _continentalnessNoiseOffsetX = UnityEngine.Random.Range(0f, 999999f);
        _continentalnessNoiseOffsetZ = UnityEngine.Random.Range(0f, 999999f);
        _temperatureNoiseOffsetX = UnityEngine.Random.Range(0f, 999999f);
        _temperatureNoiseOffsetZ = UnityEngine.Random.Range(0f, 999999f);
        _humidityNoiseOffsetX = UnityEngine.Random.Range(0f, 999999f);
        _humidityNoiseOffsetZ = UnityEngine.Random.Range(0f, 999999f);
        _heightNoiseOffsetX = UnityEngine.Random.Range(0f, 999999f);
        _heightNoiseOffsetZ = UnityEngine.Random.Range(0f, 999999f);
    }
    #endregion
}