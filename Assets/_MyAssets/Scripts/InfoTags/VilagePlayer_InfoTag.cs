using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VillagePlayer_InfoTag : NetworkBehaviour
{
    public int prefabIdPersonnage;

    public void SetPrefabIdPersonnage(int idPersonnage)
    {
        prefabIdPersonnage = idPersonnage;
        Debug.LogError("SETTING INFO TAG : " + idPersonnage);
        SetIdClientRpc(idPersonnage);
    }

    [ClientRpc]
    public void SetIdClientRpc(int idPersonnage)
    {
        Debug.LogError("SETTING INFO TAG : " + idPersonnage);
        prefabIdPersonnage = idPersonnage;
    }
}
