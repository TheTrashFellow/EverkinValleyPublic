using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;


public class Village_Player_QueryManager : NetworkBehaviour
{
    public static Village_Player_QueryManager Instance { get; private set; }

    [SerializeField] private GameObject _popUp = default;

    private Player _player;
    private Village_Player_UIManager _UIManager;


    //private Main _GameManager;

    private string apiUrl = "http://172.16.86.191:3000/actionVillagePlayer";


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
        if (IsServer)
        {
            Debug.Log("QueryManager initialized on SERVER.");
        }
        else
        {
            Debug.Log("QueryManager initialized on CLIENT.");
            _UIManager = FindAnyObjectByType<Village_Player_UIManager>();
            //_GameManager = FindAnyObjectByType<MainMenu_GameManager>();
        }
    }

    public void SetPLayer(Player player)
    {
        _player = player;
    }



    public void GetLastPosition(int idPersonnage)
    {
        GetLastPositionServerRpc(idPersonnage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetLastPositionServerRpc(int idPersonnage, ServerRpcParams rpcParams = default)
    {
        string dataJson = "{\"id_personnage\":\"" + idPersonnage + "\"}";
        StartCoroutine(SendActionMainMenu("getPositions", dataJson, rpcParams.Receive.SenderClientId));
    }

    [ClientRpc]
    private void SetPlayerPositionClientRpc(float _x, float _y, float _z, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        Vector3 _position = new(_x, _y, _z);
        _player.SetLastPositionLogin(_position);
    }

    public void SetLastPosition(Vector3 _position, int idPersonnage)
    {
        SetPlayerLastPositionServerRpc(idPersonnage, _position.x, _position.y, _position.z);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerLastPositionServerRpc(int idPersonnage, float _x, float _y, float _z, ServerRpcParams rpcParams = default)
    {
        var data = new
        {
            id_personnage = idPersonnage,
            x = _x,
            y = _y,
            z = _z
        };
        string dataJson = JsonConvert.SerializeObject(data);
        //string dataJson = "{\"id_personnage\":\"" + idPersonnage + "\", \"x\":\"" + x + "\", \"y\":\"\"" + y + "\", \"z\":\"" + z + "\"}";
        StartCoroutine(SendActionMainMenu("setPositions", dataJson, rpcParams.Receive.SenderClientId));
    }




    /*[ServerRpc(RequireOwnership = false)]
    public void AttemptLoginActionServerRpc(string courriel, string mdp, ServerRpcParams rpcParams = default)
    {

        string dataJson = "{\"courriel\":\"" + courriel + "\", \"mdp\":\"" + mdp + "\"}";
        StartCoroutine(SendActionMainMenu("attempt_login", dataJson, rpcParams.Receive.SenderClientId));
    }*/



    IEnumerator SendActionMainMenu(string action, string dataJson, ulong clientId)
    {
        Debug.Log("Sending action request : " + action + " : " + dataJson);
        string json = "{\"action\": \"" + action + "\", \"data\": " + dataJson + "}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        //StopWaitingScreenClientRpc(clientId);


        if (request.result == UnityWebRequest.Result.Success)
        {
            if (action == "getPositions")
            {
                if (string.IsNullOrWhiteSpace(request.downloadHandler.text) || request.downloadHandler.text == "[]")
                {
                    //ResultMessageClientRpc("Le courriel et ou le mot de passe entré sont érronnés." + request.error, false, clientId);
                    //Debug.Log("Response: " + request.downloadHandler.text);
                }
                else
                {
                    List<PersonnageResponse> users = JsonConvert.DeserializeObject<List<PersonnageResponse>>(request.downloadHandler.text);
                    float x = users[0].dernier_x; float y = users[0].dernier_y; float z = users[0].dernier_z;
                    SetPlayerPositionClientRpc(x, y, z, clientId);

                    Debug.Log("Response: " + request.downloadHandler.text);
                }

            }
            if (action == "setPositions")
            {

            }
        }
        else
        {
            //CreateErrorResponse response = JsonUtility.FromJson<CreateErrorResponse>(request.downloadHandler.text);
            //ResultMessageClientRpc(response.message, false, clientId);           
            Debug.LogError("Erreur avec la requête de " + action);
        }

        /*
        if (request.result == UnityWebRequest.Result.Success)
        {
           
        }
        else
        {
            
        }*/
    }

}
