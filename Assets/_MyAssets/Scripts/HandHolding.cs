using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HandHolding : NetworkBehaviour
{
    private bool isVillage = false;
    private bool isAxe = false;
    private bool isPickaxe = false;

    public bool IsAxe => isAxe;
    public bool IsPickaxe => isPickaxe;

    private GameObject itemHand = default;

    private GameObject villagePlaceholder;
    private bool isVillagePositionValid = false;

    private float placeholderRot = -1f;
    private Vector3 placeholderPos = default;


    private Player player;
    private TerrainManager terrainManager;
    private GameObject terrain;

    public GameObject ItemHand => itemHand;

    private void Start()
    {
        if (IsOwner)
        {
            //InitializeVariables();
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                ToggleAxe(!isAxe);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                TogglePickaxe(!isPickaxe);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                ServerSceneManager.Instance.ToggleVillageServerRpc(isVillage, player.personnageId, player.clientId);
            SetPlaceholder();
            PlaceVillage();
        }
    }
    /*
    private void InitializeVariables()
    {
        player = FindFirstObjectByType<Player>();
        terrainManager = FindFirstObjectByType<TerrainManager>();
    }*/

    public void SetVariables(Player _player, TerrainManager _terrainManager)
    {
        player = _player;
        terrainManager = _terrainManager;
    }

    private void ToggleAxe(bool active)
    {
        if (active)
        {
            TogglePickaxe(false);
            ToggleVillage(false);
        }

        isAxe = active;
        if (isAxe)
            itemHand = Instantiate(player.AxeItem, this.transform);
        else
        {
            Destroy(itemHand);
        }
    }

    private void TogglePickaxe(bool active)
    {
        if (active)
        {
            ToggleAxe(false);
            ToggleVillage(false);
        }

        isPickaxe = active;
        if (isPickaxe)
            itemHand = Instantiate(player.PickaxeItem, this.transform);
        else
        {
            Destroy(itemHand);
        }
    }

    public void ToggleVillage(bool active)
    {
        if (active)
        {
            ToggleAxe(false);
            TogglePickaxe(false);
        }

        isVillage = active;
        if (isVillage)
            itemHand = Instantiate(player.VillageItem, this.transform);
        else
        {
            Destroy(itemHand);
            Destroy(villagePlaceholder);
            placeholderPos = default;
            placeholderRot = -1f;
        }
    }

    private void SetPlaceholder()
    {
        if (isVillage)
        {
            Vector2 chunkPos = new(Mathf.Floor(player.transform.position.x / terrainManager.ChunkWidth), Mathf.Floor(player.transform.position.z / terrainManager.ChunkWidth));

            if (player.transform.eulerAngles.y >= 45f && player.transform.eulerAngles.y < 135f)
                InstantiatePlaceholder(chunkPos, 1, 0, 0f);
            else if (player.transform.eulerAngles.y >= 135f && player.transform.eulerAngles.y < 225f)
                InstantiatePlaceholder(chunkPos, 0, -1, 90f);
            else if (player.transform.eulerAngles.y >= 225f && player.transform.eulerAngles.y < 315f)
                InstantiatePlaceholder(chunkPos, -1, 0, 180f);
            else
                InstantiatePlaceholder(chunkPos, 0, 1, 270f);
        }
    }

    private void InstantiatePlaceholder(Vector2 chunkPos, int x, int y, float rot)
    {
        Vector3 tempPlaceholderPos = new(((chunkPos.x + x) * terrainManager.ChunkWidth) + (terrainManager.ChunkWidth / 2), 20f, ((chunkPos.y + y) * terrainManager.ChunkWidth) + (terrainManager.ChunkWidth / 2));
        float tempPlaceholderRot = rot;
        if (tempPlaceholderPos != placeholderPos || tempPlaceholderRot != placeholderRot)
        {
            Destroy(villagePlaceholder);

            RaycastHit hit;
            Vector3 origin = tempPlaceholderPos + Vector3.up * 50f; // Start the ray slightly above
            Vector3 direction = Vector3.down;

            int terrainLayer = LayerMask.GetMask("TerrainLayer");

            float height = 0f;

            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, terrainLayer))
            {
                if (hit.collider.GetComponent<Terrain>() != null)
                {
                    terrain = hit.collider.GetComponent<Terrain>().gameObject;
                    height = hit.point.y;
                }
            }

            placeholderPos = tempPlaceholderPos;
            placeholderRot = tempPlaceholderRot;
            GameObject tempPrefab;
            BiomeGeneration gen = terrain.GetComponent<BiomeGeneration>();
            if (gen.CanVillageGenerate && !gen.IsVillage)
            {
                isVillagePositionValid = true;
                tempPrefab = player.VillagePlaceholderValidPrefab;
            }
            else
            {
                isVillagePositionValid = false;
                tempPrefab = player.VillagePlaceholderInvalidPrefab;
            }

            villagePlaceholder = Instantiate(tempPrefab, new(placeholderPos.x, height, placeholderPos.z), Quaternion.Euler(0f, placeholderRot, 0f));
        }
    }

    private void PlaceVillage()
    {
        if (isVillage && isVillagePositionValid && Input.GetMouseButtonDown(0))
        {
            Terrain tempTerrain = terrain.GetComponent<Terrain>();
            BiomeGeneration biomeGeneration = tempTerrain.gameObject.GetComponent<BiomeGeneration>();
            float[,] tempHeights = biomeGeneration.GenerateTerrainForVillage(biomeGeneration.Heights);
            tempTerrain.terrainData.SetHeights(0, 0, tempHeights);

            Vector3 normal = tempTerrain.terrainData.GetInterpolatedNormal(0.5f, 0.5f);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            ServerSceneManager.Instance.CreateVillageServerRpc(villagePlaceholder.transform.position, rotation, player.personnageId);
            ToggleVillage(false);
        }
    }
}