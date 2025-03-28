using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class MainMenu_QueryManager : MonoBehaviour
{
    //public static QueryManager Instance { get; private set; }

    [SerializeField] private GameObject _popUp = default;

    private MainMenu_UIManager _UIManager;
    private MainMenu_GameManager _GameManager;

    private string apiUrl = "http://172.16.86.191:3000/actionMainMenu";

   
    /*private void Awake()
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
        }
        else
        {
            Debug.Log("QueryManager initialized on CLIENT.");
            _UIManager = FindAnyObjectByType<MainMenu_UIManager>();
            _GameManager = FindAnyObjectByType<MainMenu_GameManager>();
        }
    }*/

    private void Start()
    {
        _UIManager = FindAnyObjectByType<MainMenu_UIManager>();
        _GameManager = FindAnyObjectByType<MainMenu_GameManager>();
    }

    public void CreateAddPlayerAction(string courriel, string mdp, string nom_personnage)
    {   
        StartWaitingScreen();
        CreateAddPlayerActionServerRPC(courriel, mdp, nom_personnage);
    }

    public void AttemptLoginAction(string courriel, string mdp)
    {        
        StartWaitingScreen();
        AttemptLoginActionServerRpc(courriel, mdp);
    }
    
    private void StartWaitingScreen()
    {
        _UIManager.Show_Loading();
    }

    
    private void StopWaitingScreenClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
    {
        //if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _UIManager.Close_Loading();
    }
   

    
    private void ResultMessageClientRpc(string message, bool success, ulong clientId, ClientRpcParams clientRpcParams = default)
    {
        //if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _UIManager.Show_PopUp_Message(message, success);
    }

    
    private void Set_Errors_CreateClientRpc(string erreurCourriel, string erreurNomPerso, string erreurMDP, ulong clientId, ClientRpcParams clientRpcParams = default)
    {
       // if (NetworkManager.Singleton.LocalClientId != clientId) return;

        _UIManager.Set_Errors_Create(erreurCourriel, erreurNomPerso, erreurMDP);
    }


    
    private void StartGameClientRpc(int id_personnage)
    {
        //if (NetworkManager.Singleton.LocalClientId != clientId) return;
        _GameManager.StartGame(id_personnage);
    }
  

    public void CreateAddPlayerActionServerRPC(string courriel, string mdp, string nom_personnage, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        bool success = true;
        string erreurCourriel = "";
        string erreurNomPersonnage = "";
        string erreurMDP = "";

        //CHECK INPUT FIRST
        if(courriel.Length > 64)
        {
            StopWaitingScreenClientRpc(clientId); 
            success = false;
            erreurCourriel = "Le courriel utilisé ne peux faire plus de 64 caractères de long";
        }
        
        //Check if valid email
        
        if(nom_personnage.Length > 16)
        {
            StopWaitingScreenClientRpc(clientId);
            success = false;
            erreurNomPersonnage = "Le nom de personnage ne peux faire plus de 16 caractères de long";
        }

        //Check for basic password constraints
        
        if(success)
        {
            string hashedMDP = PasswordHasher(mdp);
            string dataJson = "{\"courriel\":\"" + courriel + "\", \"mdp\":\"" + hashedMDP + "\" , \"nom_personnage\":\"" + nom_personnage + "\"}";
            StartCoroutine(SendActionMainMenu("add_player", dataJson, clientId, null));
        }
        else
        {
            Set_Errors_CreateClientRpc(erreurCourriel, erreurNomPersonnage, erreurMDP, clientId);
        }             
    }

    /*[ServerRpc(RequireOwnership = false)]
    public void AttemptLoginActionServerRpc(string courriel, string mdp, ServerRpcParams rpcParams = default)
    {

        string dataJson = "{\"courriel\":\"" + courriel + "\", \"mdp\":\"" + mdp + "\"}";
        StartCoroutine(SendActionMainMenu("attempt_login", dataJson, rpcParams.Receive.SenderClientId));
    }*/

    
    public void AttemptLoginActionServerRpc(string courriel, string mdp, ServerRpcParams rpcParams = default)
    {

        string dataJson = "{\"courriel\":\"" + courriel + "\"}";
        StartCoroutine(SendActionMainMenu("attempt_login", dataJson, rpcParams.Receive.SenderClientId, mdp));
    }

    IEnumerator SendActionMainMenu(string action, string dataJson, ulong clientId, string mdp)
    {
        Debug.Log("Sending action request : " + action + " : " + dataJson);
        string json = "{\"action\": \"" + action + "\", \"data\": " + dataJson + "}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        StopWaitingScreenClientRpc(clientId);


        if (request.result == UnityWebRequest.Result.Success)
        {
            if (action == "add_player")
            {
                ResultMessageClientRpc("Le compte à été créé avec succès !", true, clientId);
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else if (action == "attempt_login")
            {
                if (string.IsNullOrWhiteSpace(request.downloadHandler.text) || request.downloadHandler.text == "[]")
                {
                    ResultMessageClientRpc("Le courriel et ou le mot de passe entré sont érronnés." + request.error, false, clientId);
                    Debug.Log("Response: " + request.downloadHandler.text);
                }
                else
                {
                    List<LoginResponse> users = JsonConvert.DeserializeObject<List<LoginResponse>>(request.downloadHandler.text);

                    if (VerifyPassword(mdp, users[0].mot_de_passe))                    
                        StartGameClientRpc(users[0].id_personnage);
                    else
                        ResultMessageClientRpc("Le courriel et ou le mot de passe entré sont érronnés." + request.error, false, clientId);

                    Debug.Log("Response: " + request.downloadHandler.text);
                }
            }
        }
        else
        {
            try
            {
                CreateErrorResponse response = JsonUtility.FromJson<CreateErrorResponse>(request.downloadHandler.text);
                ResultMessageClientRpc(response.message, false, clientId);
            }
            catch
            {
                ResultMessageClientRpc("Il y a eu un problème. Ne peux pas rejoindre l'API", false, clientId);
            }
                     
        }
       
        /*
        if (request.result == UnityWebRequest.Result.Success)
        {
           
        }
        else
        {
            
        }*/
    }

    public string PasswordHasher(string mdp)
    {
        const int SaltSize = 16;  // 128-bit
        const int KeySize = 32;   // 256-bit
        const int Iterations = 10000;

        byte[] salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        // Hash the password with salt
        using var pbkdf2 = new Rfc2898DeriveBytes(mdp, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(KeySize);

        // Store salt + hash as a single string (Base64 encoded)
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {        
        const int KeySize = 32;   // 256-bit
        const int Iterations = 10000;

        // Split the stored value to extract salt and hash
        string[] parts = storedHash.Split(':');
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

        // Recompute the hash with the stored salt
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(KeySize);

        // Compare hashes securely
        return CryptographicOperations.FixedTimeEquals(hash, storedHashBytes);
    }
   
}
/*
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
}*/