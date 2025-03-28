using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Drawing;
using Unity.Netcode;

public class ScenesList : MonoBehaviour
{
    public static ScenesList Instance { get; private set; }

    public List<SceneInfo> sceneInfos = new List<SceneInfo>();


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

    private void Start()
    {

        //SceneInfo openworld = new("OpenWorld", "OpenWorld", 0, initClients, new Vector3());
        //SceneInfo village = new("Village_Network", "Village_Network", 0, initClients, new Vector3());        
    }

    public void AddVillageToList(float posX, float posY, float posZ, int villageIdPersonnage, ulong prefabId, ulong scenePrefabId)
    {
        SceneInfo newPrefab = new(posX, posY, posZ, villageIdPersonnage, prefabId, scenePrefabId);
        sceneInfos.Add(newPrefab);
    }

    /*public void RemoveClientFromVillagePrefab(int villageIdPersonnage, ulong clientId)
    {
        SceneInfo prefab = sceneInfos.FirstOrDefault(scene => scene.personnageId == villageIdPersonnage);
        prefab.clientIds.Remove(clientId);

        if(prefab.clientIds.Count == 0 && !prefab.isPlayerConnected)
        {
            //ServerSceneManager.Instance.UnloadVillagePrefab(prefab.ownerClientId);
        }
    }

    public void OnPlayerDisconnected(ulong _ownerClientId)
    {
        SceneInfo prefab = sceneInfos.FirstOrDefault(scene => scene.ownerClientId == _ownerClientId);
        prefab.isPlayerConnected = false;

        if (prefab.clientIds.Count == 0)
        {
            //ServerSceneManager.Instance.UnloadVillagePrefab(_ownerClientId); 
        }
    }*/

    public SceneInfo FindPrefabByPersonnageId(int idPersonnage)
    {
        SceneInfo thisScene = sceneInfos.FirstOrDefault(prefab => prefab.personnageId == idPersonnage);
        return thisScene;
    }

    public ulong FindSceneIdByPrefabId(ulong prefabId)
    {
        SceneInfo thisScene = sceneInfos.FirstOrDefault(prefab => prefab.prefabId == prefabId);
        return thisScene.scenePrefabId;
    }

    public void SetEntryScenePrefabId(ulong scenePrefabId, int idPersonnage)
    {
        sceneInfos.FirstOrDefault(prefab => prefab.personnageId == idPersonnage);
    }

    /*
    public void AddVilageToList(string villageName, int id_personnage)
    {
        List<ulong> initClients = new List<ulong>();
        SceneInfo newEntry = new(villageName, "Village_Player", id_personnage, initClients, new Vector3());
        sceneInfos.Add(newEntry);
    }

    public void MoveClientToScene(string sceneName, ulong clientId)
    {
        SceneInfo currentScene = GetSceneInfoByClientId(clientId);
        SceneInfo thatScene = GetSceneInfoByName(sceneName);
        thatScene.clientIds.Add(clientId);
        currentScene.clientIds.Remove(clientId);       
    }*/

    /*
    public SceneInfo GetSceneInfoByName(string _sceneName)
    {
        Debug.Log("Scene Name : " + _sceneName);
        return sceneInfos.FirstOrDefault(scene => scene.sceneName == _sceneName);
    }

    public SceneInfo GetSceneInfoByClientId(ulong clientId)
    {
        return sceneInfos.FirstOrDefault(scene => scene.clientIds.Contains(clientId));
    }    */


}

[System.Serializable]
public class SceneInfo
{   
    public int personnageId;
    public ulong prefabId;
    public ulong scenePrefabId;
    public bool isPlayerConnected;
    public float position_x;
    public float position_y;
    public float position_z;

    public SceneInfo(float x, float y, float z, int _personnageId, ulong _prefabId, ulong _scenePrefabId)
    {        
        position_x = x;
        position_y = y;
        position_z = z;
        personnageId = _personnageId;
        prefabId = _prefabId;
        isPlayerConnected = true;
        scenePrefabId = _scenePrefabId;
    }
}