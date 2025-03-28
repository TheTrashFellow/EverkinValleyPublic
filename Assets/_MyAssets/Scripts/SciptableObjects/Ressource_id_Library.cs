using System.Collections.Generic;
using UnityEngine;
using static Ressource_Library;

[CreateAssetMenu(fileName = "idResourceLibrary", menuName = "Game/id Resource Library")]
public class Ressource_id_Library : ScriptableObject
{
    [System.Serializable]
    public class ResourcesSprite
    {
        public int idRessource;
        public string NomRessource;
        public Sprite _resourcePrefabs; // List of resources that can spawn in this biome        
    }


    public ResourcesSprite GetResourceById(int _idRessource)
    {
        ResourcesSprite result = default;       
                
        foreach (ResourcesSprite ressource in _ressources)
        {           
            if (ressource.idRessource == _idRessource)
            {
                return ressource;
            }            
        }
        return null;
    }

    public List<ResourcesSprite> _ressources; // List of all biome-resource pairs    
}