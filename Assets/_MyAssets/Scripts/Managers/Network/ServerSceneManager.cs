using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ServerSceneManager : NetworkBehaviour
{
    public static ServerSceneManager Instance { get; private set; }

    [SerializeField] private GameObject _villagePrefab;
    [SerializeField] private GameObject _villageScene;

    private Player _player;

    private Vector3 villageNPCLocation = new Vector3(89, 1324, 47);

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

    public void SetPLayer(Player player)
    {
        _player = player;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        //scenesList = ScenesList.Instance;
        //Debug.Log(scenesList);
    }

    /*public void UnloadVillagePrefab(ulong clientId)
    {
        Debug.Log("In UnloadVillagePrefab");
        SceneInfo prefabInfo = ScenesList.Instance.sceneInfos.FirstOrDefault(prefab => prefab.ownerClientId == clientId);
        Debug.Log(prefabInfo.prefabId);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(prefabInfo.prefabId, out var spawnedObject))
        {           
            spawnedObject.Despawn();
        }
        else
        {
            Debug.LogError("NE DEVRAIS PAS APPARAITRE !!! VOIR SERVER SCENE MANAGER : UnloadVillagePrefab");
        }
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectServerRpc(ulong clientId)
    { 
        /*
        Debug.Log("Looking up scene information to add client : " + clientId);
        Debug.Log(scenesList.ToString());
        scenesList.GetSceneInfoByName("OpenWorld").clientIds.Add(clientId);
        */
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveClientToSceneServerRpc(ulong clientId, string sceneName)
    {           
        //NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        
        StartCoroutine(WaitAndMovePlayer(clientId, sceneName));
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveClientFromPrefabToSceneServerRpc(ulong clientId, ulong prefabId)
    {
        StartCoroutine(WaitAndMoveToVillageScene(clientId, prefabId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveClientFromSceneToPrefabServerRpc(ulong clientId, ulong sceneId)
    {
        StartCoroutine(WaitAndMoveToVillagePrefab(clientId, sceneId));
    }

    [ClientRpc]
    private void RequestStopTerrainGenerationClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _player.StopTerrainGeneration();
    }

    [ClientRpc]
    private void RequestMovePlayerInWorldSpaceClientRpc(ulong clientId, Vector3 position, string sceneName)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        //Start Loading screen

        _player.MovePlayerInWorldSpace(position, sceneName);
    }

    [ClientRpc]
    private void RequestLastPositionClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        OpenWorld_QueryManager.Instance.GetLastPosition(_player.personnageId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowVillageToClientServerRpc(ulong clientId, ulong villageId)
    {
        Debug.LogError("ShowVillageToClientServerRpc work!");
        List<SceneInfo> ressources = ScenesList.Instance.sceneInfos;
        SceneInfo info = ressources.Find(item => item.prefabId == villageId);
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(info.prefabId, out var spawnedObject))
        {
            spawnedObject.NetworkShow(clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HideVillageToClientServerRpc(Vector2 villagePos, ulong clientId)
    {
        Debug.LogError("HideVillageToClientServerRpc work!");

        List<SceneInfo> infos = ScenesList.Instance.sceneInfos;
        SceneInfo info = infos.Find(item => item.position_x == villagePos.x && item.position_z == villagePos.y);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(info.prefabId, out var spawnedObject))
        {
            spawnedObject.NetworkHide(clientId);
        }
    }

    [ClientRpc]
    public void RefreshChunkClientRpc(Vector3 villagePos, ulong villageId)
    {
        RaycastHit hit;
        Vector3 origin = villagePos + Vector3.up * 10f; // Start the ray slightly above
        Vector3 direction = Vector3.down;

        int terrainLayer = LayerMask.GetMask("TerrainLayer");

        try
        {
            GameObject tempTerrain = default;
            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, terrainLayer))
            {
                tempTerrain = hit.collider.gameObject;
                TerrainManager.Instance.RefreshChunk(Vector2Int.RoundToInt(TerrainManager.Instance.GetTerrainChunkPos(tempTerrain)));
                ShowVillageToClientServerRpc(NetworkManager.Singleton.LocalClientId, villageId);
            }
        } catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void FindVillageForChunkServerRpc(Vector2 villagePos, ulong clientId)
    {
        List<SceneInfo> ressources = ScenesList.Instance.sceneInfos;
        SceneInfo info = ressources.Find(item => item.position_x == villagePos.x && item.position_z == villagePos.y);
        if (info != null)
            RefreshChunkOfPlayerClientRpc(villagePos, info.prefabId, clientId);
    }

    [ClientRpc]
    public void RefreshChunkOfPlayerClientRpc(Vector2 villagePos, ulong prefabId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        
        Debug.LogError("work!");
        RaycastHit hit;
        Vector3 origin = new Vector3(villagePos.x, 0f, villagePos.y) + Vector3.up * 100f; // Start the ray slightly above
        Vector3 direction = Vector3.down;
        int terrainLayer = LayerMask.GetMask("TerrainLayer");
        try
        {
            Debug.LogError("Terrain work!");
            GameObject tempTerrain = default;
            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, terrainLayer))
            {
                tempTerrain = hit.collider.gameObject;
                TerrainManager.Instance.RefreshChunk(Vector2Int.RoundToInt(TerrainManager.Instance.GetTerrainChunkPos(tempTerrain)));
                ShowVillageToClientServerRpc(clientId, prefabId);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateVillageServerRpc(Vector3 villagePos, Quaternion villageRot, int personnageId)
    {
        GameObject tempVillage = Instantiate(_villagePrefab, villagePos, villageRot);
        tempVillage.GetComponent<NetworkObject>().Spawn();
        ulong objectId = tempVillage.GetComponent<NetworkObject>().NetworkObjectId;

        GameObject sceneVillage = Instantiate(_villageScene, new Vector3(0f, 100f, 0f), Quaternion.Euler(0f, 0f, 0f));
        sceneVillage.GetComponent<NetworkObject>().Spawn();
        ulong scenePrefabId = sceneVillage.GetComponent<NetworkObject>().NetworkObjectId;

        //tempVillage.GetComponent<VillagePlayer_InfoTag>().SetPrefabIdPersonnage(personnageId);

        OpenWorld_QueryManager.Instance.PlayerSetVillagePositions(personnageId, villagePos.x, villagePos.y, villagePos.z);
        ScenesList.Instance.AddVillageToList(villagePos.x, villagePos.y, villagePos.z, personnageId, objectId, scenePrefabId);
        RefreshChunkClientRpc(villagePos, objectId);
    }

    public void CreateVillage(Vector3 villagePos, Quaternion villageRot, int personnageId)
    {
        GameObject smallVillage = Instantiate(_villagePrefab, villagePos, villageRot);
        smallVillage.GetComponent<NetworkObject>().Spawn();
        ulong smallVillageId = smallVillage.GetComponent<NetworkObject>().NetworkObjectId;

        GameObject sceneVillage = Instantiate(_villageScene, new Vector3(0f, 100f, 0f), Quaternion.Euler(0f, 0f, 0f));
        sceneVillage.GetComponent<NetworkObject>().Spawn();
        ulong scenePrefabId = sceneVillage.GetComponent<NetworkObject>().NetworkObjectId;

        //tempVillage.GetComponent<VillagePlayer_InfoTag>().SetPrefabIdPersonnage(personnageId);

        ScenesList.Instance.AddVillageToList(villagePos.x, villagePos.y, villagePos.z, personnageId, smallVillageId, scenePrefabId);
        RefreshChunkClientRpc(villagePos, smallVillageId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleVillageServerRpc(bool isVillage, int personnageId, ulong clientId)
    {
        List<SceneInfo> ressources = ScenesList.Instance.sceneInfos;
        SceneInfo info = ressources.Find(item => item.personnageId == personnageId);
        if (info == null)
        {
            ToggleVillageClientRpc(isVillage, clientId);
        }
    }

    [ClientRpc]
    public void ToggleVillageClientRpc(bool isVillage, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _player.GetComponentInChildren<HandHolding>().ToggleVillage(!isVillage);
        /*Debug.LogError(personnageId);
        tempVillage.GetComponent<VillagePlayer_InfoTag>().SetPrefabIdPersonnage(personnageId);

        ScenesList.Instance.AddVillageToList(villagePos.x, villagePos.y, villagePos.z, personnageId, clientId, objectId);
        RefreshVillageClientRpc(villagePos, objectId);*/
    }

    [ClientRpc]
    private void ChangeMusicClientRpc(ulong clientId, string songName)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _player.SelectSceneMusic(false, songName);
        
    }


    //FONCTIONNEL ! PEUT ETRE ENLEVER DE LA LISTE DE SCENES. 
    private System.Collections.IEnumerator WaitAndMovePlayer(ulong clientId, string sceneName)
     {
        yield return new WaitForSeconds(1f); // Small delay for scene load

        // Find the player�s NetworkObject
        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObject != null)
        {
            // Move the player's GameObject to the new scene
            SceneManager.MoveGameObjectToScene(playerObject.gameObject, SceneManager.GetSceneByName(sceneName));
                    

            Vector3 position = Vector3.zero;

            if(sceneName == "Village_Network")
            {
                ChangeMusicClientRpc(clientId, "Village_network");
                Debug.Log("Player object : " + playerObject.GetComponent<Player>().clientId);
                position = villageNPCLocation;                
                RequestStopTerrainGenerationClientRpc(clientId);
                RequestMovePlayerInWorldSpaceClientRpc(clientId, position, sceneName);
                //playerObject.GetComponent<Player>().MovePlayerInWorldSpaceClientRpc(position, sceneName);
            }
            else if(sceneName == "OpenWorld")
            {
                ChangeMusicClientRpc(clientId, "OpenWorld");
                //position = playerObject.GetComponent<Player>().RequestQueryLastPosition();
                RequestLastPositionClientRpc(clientId);
                
            }
            
            //ScenesList.Instance.GetSceneInfoByClientId(clientId).clientIds.Remove(clientId);
            //ScenesList.Instance.GetSceneInfoByName(sceneName).clientIds.Add(clientId);

            Debug.Log($"Client {clientId} moved to {sceneName}");
        }
        else
        {
            Debug.LogError($"Failed to move client {clientId}. PlayerObject not found!");
        }
    }
    private System.Collections.IEnumerator WaitAndMoveToVillageScene(ulong clientId, ulong prefabId)
    {
        yield return new WaitForSeconds(1f);

        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObject != null)
        { 
            ulong ScenePrefabId = ScenesList.Instance.FindSceneIdByPrefabId(prefabId);

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ScenePrefabId, out var prefab))
            {
                prefab.NetworkShow(clientId);
            }

            Vector3 position = new Vector3(-7.5f, 78f, -12f);

            RequestStopTerrainGenerationClientRpc(clientId);
            RequestMovePlayerInWorldSpaceClientRpc(clientId, position, "Prefab");

        }
        else
        {
            Debug.LogError($"Failed to move client {clientId}. PlayerObject not found!");
        }
    }

    private System.Collections.IEnumerator WaitAndMoveToVillagePrefab(ulong clientId, ulong sceneId)
    {
        yield return new WaitForSeconds(1f);

        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObject != null)
        {
            RequestLastPositionClientRpc(clientId);

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(sceneId, out var prefab))
            {
                prefab.NetworkHide(clientId);
            }
        }
        else
        {
            Debug.LogError($"Failed to move client {clientId}. PlayerObject not found!");
        }
    }

}
