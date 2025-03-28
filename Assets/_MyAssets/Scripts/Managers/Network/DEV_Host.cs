using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DEV_Host : MonoBehaviour
{   
    void Start()
    {
        NetworkManager.Singleton.StartHost();
    }

    
}
