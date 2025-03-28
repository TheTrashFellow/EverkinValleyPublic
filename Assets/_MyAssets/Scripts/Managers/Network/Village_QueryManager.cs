using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class Village_QueryManager : NetworkBehaviour
{
    public static Village_QueryManager Instance { get; private set; }

    //[SerializeField] private GameObject _popUp = default;

    private Village_UIManager _UIManager;
    private Village_GameManager _GameManager;

    private string apiUrl = "http://172.16.86.191:3000/actionVillage";

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
            _UIManager = FindAnyObjectByType<Village_UIManager>();
            _GameManager = FindAnyObjectByType<Village_GameManager>();
        }
    }

    // Envoi de la recette 
    public void sendRecipeToNpc(RecettesResponse recette, ulong clientId)
    {
        if (recette.id_batiment.HasValue) // Si id_batiment a une valeur => bâtiment (charpentier)
        {
            SendRecipeToCarpenter(JsonConvert.SerializeObject(recette), clientId);
        }
        else // Si id_batiment est null => outil (forgeron)
        {
            SendRecipeToBlackSmith(JsonConvert.SerializeObject(recette), clientId);
        }
    }

    // Recette bâtiments/Charpentier
    private void SendRecipeToCarpenter(string recetteJson, ulong clientId)
    {
        Debug.Log("Envoi recette Charpentier : " + recetteJson);
        StartCoroutine(SendActionVillage("carpenter_build", recetteJson, clientId));
    }

    // Recette outils/Forgeron
    private void SendRecipeToBlackSmith(string recetteJson, ulong clientId)
    {
        Debug.Log("Envoi recette Forgeron : " + recetteJson);
        StartCoroutine(SendActionVillage("blacksmith_build", recetteJson, clientId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCraftRecipeServerRpc(int idPersonnage, int recetteId, ServerRpcParams rpcParams = default)
    {

    }

    IEnumerator SendActionVillage(string action, string dataJson, ulong clientId)
    {
        Debug.Log("Sending action request : " + action + " : " + dataJson);
        string json = "{\"action\": \"" + action + "\", \"data\": " + dataJson + "}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Réponse du serveur " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Erreur lors de l'envoi de l'action " + action + " : " + request.error);
        }
    }

    public class BatimentResponse
    {
        public int id_batiment { get; set; }
        public string nom_batiment { get; set; }
        public string amelioration { get; set; }
    }

    public class RecettesResponse
    {
        public int id_recette { get; set; }
        public string nom_recette { get; set; }
        public bool est_amelioration { get; set; }
        public string a_ameliorer { get; set; }
        public int nouveau_niveau { get; set; }
        // Null pour un outil
        public int? id_batiment { get; set; }
    }

    public class RessourcesResponse
    {
        public int id_ressource { get; set; }
        public string nom_ressource { get; set; }
    }

    public class BundlesResponse
    {
        public int id_ressource { get; set; }
        public int id_recette { get; set; }
        public int quantite { get; set; }
    }
}
