using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenWorld_GameManager : NetworkBehaviour
{
    public static OpenWorld_GameManager Instance { get; private set; }

    private Player _player;

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
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        Debug.Log("SUBSCRIBED TO Client disconnect");
    }
     

    public void SetPlayer(Player player)
    {
        _player = player;
        /*InstantiateVillageServerRpc(_player.personnageId);*/
    }

    /*[ServerRpc(RequireOwnership = false)]
    public void InstantiateVillageServerRpc(int idPersonnage, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameObject thisVillage = Instantiate(_village);
        NetworkObject netObject = thisVillage.GetComponent<NetworkObject>();
        netObject.GetComponent<VillagePlayer_InfoTag>().SetPrefabIdPersonnage(idPersonnage);

        netObject.Spawn();
        
        ulong prefabId = netObject.NetworkObjectId;
        ScenesList.Instance.AddVillageToList(idPersonnage, clientId, prefabId);
    }*/

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log("In Client Disconnect");
        //ScenesList.Instance.OnPlayerDisconnected(clientId);
    }

    public void DisconnectPlayer() 
    {
        OpenWorld_QueryManager.Instance.SetLastPosition(_player.GetPosition(), _player.personnageId);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }
}
