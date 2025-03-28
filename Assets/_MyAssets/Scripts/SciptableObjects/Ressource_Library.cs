using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeResourceLibrary", menuName = "Game/Biome Resource Library")]
public class Ressource_Library : ScriptableObject
{
    [System.Serializable]
    public class ResourcesRarity
    {
        public int _ressourceId;
        public GameObject _resourcePrefabs; // List of resources that can spawn in this biome
        public float _rarity;
    }

    [System.Serializable]
    public class BiomeResources
    {
        public string biomeName; // Name of the biome
        public List<ResourcesRarity> _resource; // List of resources that can spawn in this biome
        
    }

    public List<BiomeResources> _biomes; // List of all biome-resource pairs

    // Get resources for a specific biome
    public GameObject GetResourceForBiome(int biome)
    {
        GameObject result = default;
        float rarity = Random.Range(0f, 1f-0.0001f);

        float rare = 0f;
        int index = 0;
        foreach (ResourcesRarity ressource in _biomes[biome]._resource)
        {
            rare += ressource._rarity;
            if (rarity < rare)
            {
                result = _biomes[biome]._resource[index]._resourcePrefabs;
                break;
            }
            index++;
        }

        if (result != null)
        {
            return result;
        }

        return null;
    }
}