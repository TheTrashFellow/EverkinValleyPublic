using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;


public class OpenWorld_QueryManager: NetworkBehaviour
{
    public static OpenWorld_QueryManager Instance { get; private set; }

    [SerializeField] private GameObject _popUp = default;

    private Player _player;
    private bool _isInventoryFetched;

    private MainScene_UIManager _UIManager;
    //private Main _GameManager;

    private string apiUrl = "http://172.16.86.191:3000/actionOpenWorld";

   
    private void Awake()
    {
        if(Instance == null)
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
            SetAllVillages();
        }
        else
        {
            Debug.Log("QueryManager initialized on CLIENT.");
            _UIManager = FindAnyObjectByType<MainScene_UIManager>();
            _isInventoryFetched = false;    
            //_GameManager = FindAnyObjectByType<MainMenu_GameManager>();
        }
    }

    public void SetPLayer(Player player)
    {
        _player = player;
        FetchPlayerInventoryServerRpc(player.personnageId);
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

    [ClientRpc]
    private void SetPlayerToolsClientRpc(int hache, int pioche, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _player.SetTools(hache, pioche);
    }

    private void SetAllVillages(ServerRpcParams rpcParams = default)
    {
        var data = new
        {

        };
        string dataJson = JsonConvert.SerializeObject(data);
        StartCoroutine(SendActionMainMenu("fetch_villages", dataJson, rpcParams.Receive.SenderClientId));
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

    [ServerRpc(RequireOwnership = false)]
    public void FetchPlayerInventoryServerRpc(int idPersonnage, ServerRpcParams rpcParams = default)
    {
        string dataJson = "{\"id_personnage\":\"" + idPersonnage + "\"}";
        StartCoroutine(SendActionMainMenu("fetch_inventory", dataJson, rpcParams.Receive.SenderClientId));
    }

    private void FetchPlayerInventory(int idPersonnage, ulong idClient)
    {
        string dataJson = "{\"id_personnage\":\"" + idPersonnage + "\"}";
        StartCoroutine(SendActionMainMenu("fetch_inventory", dataJson, idClient));
    }

    [ClientRpc]
    private void SetPlayerInventoryClientRpc(ulong clientId, int idRessource, int quantite, int maximum)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        if (!_isInventoryFetched)
        {                     
            _player.SetInventory(idRessource, quantite, maximum);
        }
        else
        {
            _player.ModifyInventory(idRessource, quantite, maximum);
        }
    }

    [ClientRpc]
    private void PlayerIsInventorySetClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _isInventoryFetched = true;
    }

    [ClientRpc]
    private void SetPlayerNameTagClientRpc(string playerName, ulong clientid)
    {
        _player.SetNameTag(playerName);
    }


    public void PlayerGotRessource(int _idRessource, int _quantity, ulong _idClient, int _idPersonnage)
    {        
        string dataJson = "{\"id_personnage\":\"" + _idPersonnage + "\", \"id_ressource\":\"" + _idRessource + "\" , \"quantite\":\"" + _quantity + "\"}";
        StartCoroutine(SendActionMainMenu("got_ressource", dataJson, _idClient));
    }

    [ClientRpc]
    private void ResetPlayerInventoryClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _player.ResetInventory();
    }

    [ClientRpc]
    private void FetchPlayerInformationClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        FetchPlayerInformationServerRpc(clientId, _player.personnageId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FetchPlayerInformationServerRpc(ulong clientId, int personnageId)
    {
        string dataJson = "{\"id_personnage\":\"" + personnageId + "\"";
        StartCoroutine(SendActionMainMenu("fetch_information", dataJson, clientId));
    }

    [ServerRpc(RequireOwnership = false )]
    public void PlayerCraftRecipeServerRpc(int _idPersonnage, int _idRecette, ulong _idClient)
    {
        string dataJson = "{\"id_personnage\":\"" + _idPersonnage + "\", \"id_recette\":\"" + _idRecette + "\"}";
        StartCoroutine(SendActionMainMenu("craft_recette", dataJson, _idClient));
    }

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
                    //ResultMessageClientRpc("Le courriel et ou le mot de passe entr� sont �rronn�s." + request.error, false, clientId);
                    //Debug.Log("Response: " + request.downloadHandler.text);
                }
                else
                {
                    List<PersonnageResponse> users = JsonConvert.DeserializeObject<List<PersonnageResponse>>(request.downloadHandler.text);
                    float x = users[0].dernier_x; float y = users[0].dernier_y; float z = users[0].dernier_z;
                    int niveauHache = users[0].niveau_hache;
                    int niveauPioche = users[0].niveau_pioche;
                    string userName = users[0].nom_personnage;
                    SetPlayerPositionClientRpc(x, y, z, clientId);
                    SetPlayerToolsClientRpc(niveauHache, niveauPioche, clientId);
                    SetPlayerNameTagClientRpc(userName, clientId);

                    Debug.Log("Response: " + request.downloadHandler.text);
                }
                
            }           
            if (action == "fetch_information")
            {
                List<PersonnageResponse> users = JsonConvert.DeserializeObject<List<PersonnageResponse>>(request.downloadHandler.text);
                int niveauHache = users[0].niveau_hache;
                int niveauPioche = users[0].niveau_pioche;
                SetPlayerToolsClientRpc(niveauHache, niveauPioche, clientId);
            }
            if (action == "fetch_inventory")
            {
                List<InventoryResponse> inventory = JsonConvert.DeserializeObject<List<InventoryResponse>>(request.downloadHandler.text);
                foreach (InventoryResponse inventoryItem in inventory)
                {
                    SetPlayerInventoryClientRpc(clientId, inventoryItem.id_ressource, inventoryItem.quantite, inventoryItem.maximum);
                }
                PlayerIsInventorySetClientRpc(clientId);
            }
            if(action == "got_ressource")
            {
                GotRessourceResponse idPersonnage = JsonConvert.DeserializeObject<GotRessourceResponse>(request.downloadHandler.text);
                FetchPlayerInventory(idPersonnage.idPersonnage, clientId);
            }
            if(action == "craft_recette")
            {
                Debug.LogError("Recette crafted !!!");
                ResetPlayerInventoryClientRpc(clientId);     
                FetchPlayerInformationClientRpc(clientId);
            }       
            if(action == "setVillagePositions")
            {

            }
            if (action == "fetch_villages")
            {
                List<PersonnageResponse> users = JsonConvert.DeserializeObject<List<PersonnageResponse>>(request.downloadHandler.text);
                // CONTINUE HERE TO LOAD THEM
                for (int i = 0; i < users.Count; i++)
                {
                    Debug.LogError("x : " + users[i].village_dernier_x);
                    ServerSceneManager.Instance.CreateVillage(new Vector3(users[i].village_dernier_x, users[i].village_dernier_y, users[i].village_dernier_z), Quaternion.Euler(0f, 0f, 0f), users[i].id_personnage);
                }
                Debug.LogError("Sa fonctionne! : " + action);
            }
        }
        else
        {
            //CreateErrorResponse response = JsonUtility.FromJson<CreateErrorResponse>(request.downloadHandler.text);
            //ResultMessageClientRpc(response.message, false, clientId);           
            Debug.LogError("Erreur avec la requ�te de " + action);
        }
       
        /*
        if (request.result == UnityWebRequest.Result.Success)
        {
           
        }
        else
        {
            
        }*/
    }

    public void PlayerSetVillagePositions(int _idPersonnage, float _villageX, float _villageY, float _villageZ, ServerRpcParams rpcParams = default)
    {
        var data = new
        {
            id_personnage = _idPersonnage,
            village_dernier_x = _villageX,
            village_dernier_y = _villageY,
            village_dernier_z = _villageZ
        };
        string dataJson = JsonConvert.SerializeObject(data);
        StartCoroutine(SendActionMainMenu("setVillagePositions", dataJson, rpcParams.Receive.SenderClientId));
    }
}

public class PersonnageResponse
{
    public int id_personnage { get; set; }
    public int niveau_hache { get; set; }
    public int niveau_pioche { get; set; }
    public string nom_village { get; set; }
    public float dernier_x { get; set; }
    public float dernier_y { get; set; }
    public float dernier_z { get; set; }
    public float derniere_rotation_y { get; set; }
    public float village_dernier_x { get; set; }
    public float village_dernier_y { get; set; }
    public float village_dernier_z { get; set; }
    public string nom_personnage { get; set; }

}

public class InventoryResponse
{
    public int id_personnage;
    public int id_ressource;
    public int quantite;
    public int maximum;
}

public class LoginResponse
{
    public int id_utilisateur { get; set; }
    public string nom_utilisateur { get; set; }
    public string mot_de_passe { get; set; }
    public int id_personnage { get; set; }
}


public class CreateErrorResponse
{
    public string message;
    public ErrorDetails details;
}


public class ErrorDetails
{
    public string code;
    public int errno;
    public string sqlState;
    public string sqlMessage;
    public string sql;
}

public class GotRessourceResponse
{
    public int idPersonnage;
}