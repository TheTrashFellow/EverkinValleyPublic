using UnityEngine;
using System.Collections.Generic;
using static Ressource_id_Library;

[CreateAssetMenu(fileName = "BatimentLibrary", menuName = "Game/Batiment Library")]
public class Batiment_Library : ScriptableObject
{
    [System.Serializable]
    public class BatimentData
    {
        public int idBatiment;  
        public string nomBatiment;
        public GameObject batiment;     
        public GameObject batimentValid;
        public GameObject batimentInvalid;
        public Sprite batimentSprite;
        public string amelioration;
    }

    public BatimentData GetBatimentById(int _idBatiment)
    {
        BatimentData result = default;

        foreach (BatimentData batiment in batiments)
        {
            if (batiment.idBatiment == _idBatiment)
            {
                return batiment;
            }
        }
        return null;
    }

    public List<BatimentData> batiments = new List<BatimentData>(); // Liste de toutes les recettes

}

