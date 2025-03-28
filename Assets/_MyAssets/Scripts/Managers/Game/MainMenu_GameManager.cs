using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class MainMenu_GameManager : MonoBehaviour
{
    private MainMenu_QueryManager queryManager;
    private Network_Client network_Client;

    private void Start()
    {
        queryManager = FindAnyObjectByType<MainMenu_QueryManager>();
        network_Client = FindAnyObjectByType<Network_Client>();
    }

    /*
    public override void OnNetworkSpawn()
    {        
         queryManagerInstance = QueryManager.GetLocalInstance();
    }*/
    /*
    private void SpawnClientQueryManager(ulong clientId)
    {
        Debug.Log($"Spawning QueryManager for client {clientId}");

        GameObject obj = Instantiate(queryManagerPrefab);
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();

        networkObject.SpawnWithOwnership(clientId); // Assign ownership to the client
    }*/

    /*
    private void SpawnQueryManager()
    {
        if (queryManagerInstance == null)
        {
            GameObject obj = Instantiate(queryManagerPrefab);
            obj.GetComponent<NetworkObject>().Spawn();
            queryManagerInstance = obj.GetComponent<QueryManager>();
        }
    }*/


    public void CreateAccount(string courriel, string mdp, string nomPerso)
    {    
        queryManager.CreateAddPlayerAction(courriel, mdp, nomPerso);
    }

    public void StartNewGame()
    {
       
    }

    public void StartGame(int id_personnage)
    {
        Debug.LogError("Starting the game !");
        PersonnageId_Reference.PersonnageId = id_personnage;
        network_Client.StartClient();
        //ulong clientId = NetworkManager.LocalClientId;
        //ServerSceneManager.Instance.MoveClientToSceneServerRpc(clientId, "OpenWorld");
    }

    public void AttemptLogin(string courriel, string mdp)
    {
        queryManager.AttemptLoginAction(courriel, mdp);
    }

}
