using UnityEngine;
using Unity.Netcode;

public class Network_Client : MonoBehaviour
{   

    public void StartClient()
    {
        Debug.LogError("In start client");
        NetworkManager.Singleton.StartClient();
        Debug.LogError("Now connected ? ");
    }

}