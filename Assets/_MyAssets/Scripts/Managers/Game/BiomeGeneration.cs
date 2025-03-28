using UnityEngine;

public class BiomeGeneration : MonoBehaviour
{
    #region |-- VARIABLES --|
    private Terrain terrain;
    private TerrainManager terrainManager;
    private float[,] heights;
    private bool canVillageGenerate = true;
    private bool isVillage = false;
    private float flattenStrength = 5f;
    private int smoothRadius = 25;
    private GameObject villageBuilding;

    private bool forceVillage = false;

    public bool CanVillageGenerate => canVillageGenerate;
    public bool IsVillage => isVillage;

    public float[,] Heights => heights;
    #endregion

    #region |-- UNITY METHODS --|
    void Start()
    {        
        InitializeVariables();
        if(terrain != null)
        {
            GenerateBiome();
            if (!isVillage)
            {
                terrainManager.GenerateTree(new Vector2Int((int)terrain.transform.position.x, (int)terrain.transform.position.z), heights);
            }
        }
    }
    #endregion

    #region |-- BIOME GENERATION METHODS --|
    private void GenerateVillage()
    {
        if (canVillageGenerate)
        {
            int seed = terrainManager.Seed;
            int x = (int)this.transform.position.x + (terrainManager.ChunkWidth / 2);
            int z = (int)this.transform.position.z + (terrainManager.ChunkWidth / 2);
            int xz = (x >= z) ? (x * x + x + z) : (z * z + x);
            int newSeed = (xz >= seed) ? (xz * xz + xz + seed) : (seed * seed + xz);
            Random.InitState(newSeed);
            float rate = Random.Range(0f, 1f);
            if (rate <= terrainManager.VillageRate)
            {
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
                bool generate = true;

                for (int a = -terrainManager.VillageMinimumDistance; a <= terrainManager.VillageMinimumDistance; a++)
                {
                    for (int b = -terrainManager.VillageMinimumDistance; b <= terrainManager.VillageMinimumDistance; b++)
                    {
                        if (!(a == 0 && b == 0))
                        {
                            int nearX = (int)this.transform.position.x + (terrainManager.ChunkWidth / 2) + (b * terrainManager.ChunkWidth);
                            int nearZ = (int)this.transform.position.z + (terrainManager.ChunkWidth / 2) + (a * terrainManager.ChunkWidth);
                            int nearXZ = (nearX >= nearZ) ? (nearX * nearX + nearX + nearZ) : (nearZ * nearZ + nearX);
                            int nearSeed = (nearXZ >= seed) ? (nearXZ * nearXZ + nearXZ + seed) : (seed * seed + nearXZ);
                            Random.InitState(nearSeed);
                            float nearVillage = Random.Range(0f, 1f);
                            if (nearVillage <= terrainManager.VillageRate)
                            {
                                generate = false;
                                break;
                            }
                        }
                    }
                    if (!generate) break;
                }

                if (generate)
                {
                    villageBuilding = Instantiate(terrainManager.Village, new Vector3(x, 0f, z), rotation);
                    villageBuilding.transform.SetParent(this.transform);
                    isVillage = true;
                }
            }
            if (isVillage)
            {
                heights = GenerateTerrainForVillage(heights);
                villageBuilding.transform.position = new Vector3(villageBuilding.transform.position.x, heights[(int)((heights.GetLength(0) - 1) / 2f), (int)((heights.GetLength(1) - 1) / 2f)] * terrainManager.ChunkHeight, villageBuilding.transform.position.z);
            }
        }
    }

    public float[,] GenerateTerrainForVillage(float[,] _heights)
    {
        int width = _heights.GetLength(0) - 1;
        int height = _heights.GetLength(1) - 1;

        float maxDistance = Mathf.Min(width, height) / 2f; // Distance from edges
        float centerX = width / 2f;
        float centerY = height / 2f;

        for (int flattenX = 0; flattenX < width; flattenX++)
        {
            for (int flattenY = 0; flattenY < height; flattenY++)
            {
                float distX = Mathf.Abs(flattenX - centerX);
                float distY = Mathf.Abs(flattenY - centerY);
                float distance = Mathf.Max(distX, distY); // Distance from closest edge

                float flattenFactor = Mathf.Clamp01(distance / maxDistance);
                flattenFactor = Mathf.Pow(flattenFactor, 2); // Exponential effect

                float smoothedHeight = GetSmoothedHeight(_heights, flattenX, flattenY, smoothRadius);
                _heights[flattenX, flattenY] = Mathf.Lerp(_heights[flattenX, flattenY], smoothedHeight, flattenStrength * (1 - flattenFactor));
            }
        }

        return _heights;
    }

    private void GenerateBiome()
    {
        TerrainLayer[] terrainLayers = new TerrainLayer[terrainManager.Biomes.Length];
        for (int i = 0; i < terrainManager.Biomes.Length; i++)
        {
            terrainLayers[i] = terrainManager.Biomes[i].TerrainTexture;
        }
        float[,,] splatmapData = new float[terrainManager.ChunkResolution, terrainManager.ChunkResolution, terrainLayers.Length];
        heights = new float[terrainManager.ChunkResolution + 1, terrainManager.ChunkResolution + 1];
        for (int y = 0; y <= terrainManager.ChunkResolution; y++)
        {
            for (int x = 0; x <= terrainManager.ChunkResolution; x++)
            {
                float continentalness = GenerateLayer(x, y, transform.position.x, transform.position.z, terrainManager.ContinentalnessBorderCurve, terrainManager.ContinentalnessNoiseScale, terrainManager.ContinentalnessNoiseOffsetX, terrainManager.ContinentalnessNoiseOffsetZ, terrainManager.ContinentalnessFrequency, terrainManager.ContinentalnessOctaves, terrainManager.ContinentalnessRoughness, terrainManager.ContinentalnessLacunarity, terrainManager.ContinentalnessGenerationPercentRange, terrainManager.ContinentalnessCutPercentRange, terrainManager.ContinentalnessIsReverse);
                float temperature = GenerateLayer(x, y, transform.position.x, transform.position.z, terrainManager.TemperatureBorderCurve, terrainManager.TemperatureNoiseScale, terrainManager.TemperatureNoiseOffsetX, terrainManager.TemperatureNoiseOffsetZ, terrainManager.TemperatureFrequency, terrainManager.TemperatureOctaves, terrainManager.TemperatureRoughness, terrainManager.TemperatureLacunarity, terrainManager.TemperatureGenerationPercentRange, terrainManager.TemperatureCutPercentRange, terrainManager.TemperatureIsReverse);
                float humidity = GenerateLayer(x, y, transform.position.x, transform.position.z, terrainManager.HumidityBorderCurve, terrainManager.HumidityNoiseScale, terrainManager.HumidityNoiseOffsetX, terrainManager.HumidityNoiseOffsetZ, terrainManager.HumidityFrequency, terrainManager.HumidityOctaves, terrainManager.HumidityRoughness, terrainManager.HumidityLacunarity, terrainManager.HumidityGenerationPercentRange, terrainManager.HumidityCutPercentRange, terrainManager.HumidityIsReverse);

                int currentBiome = ApplyBiome(continentalness, temperature, humidity);

                if (currentBiome == 0 || currentBiome == 1 && canVillageGenerate)
                {
                    canVillageGenerate = false;
                }

                if (x != terrainManager.ChunkResolution && y != terrainManager.ChunkResolution)
                {
                    for (int layer = 0; layer < terrainLayers.Length; layer++)
                    {
                        splatmapData[y, x, layer] = layer == currentBiome ? 1f : 0f;
                    }
                }
                heights[y, x] = GenerateHeight(x, y, continentalness, temperature, humidity);
            }
        }
        if (forceVillage)
        {
            isVillage = true;
            heights = GenerateTerrainForVillage(heights);
        }
        else
        {
            GenerateVillage();
        }

        terrain.terrainData.SetHeights(0, 0, heights);
        terrain.terrainData.terrainLayers = terrainLayers;
        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);

        if (isVillage && villageBuilding)
        {
            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(0.5f, 0.5f);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            villageBuilding.transform.rotation = rotation;
        }
    }

    float GetSmoothedHeight(float[,] heights, int x, int y, int radius)
    {
        int width = heights.GetLength(0);
        int height = heights.GetLength(1);
        float sum = 0;
        int count = 0;

        for (int offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                int nx = Mathf.Clamp(x + offsetX, 0, width - 1);
                int ny = Mathf.Clamp(y + offsetY, 0, height - 1);
                sum += heights[nx, ny];
                count++;
            }
        }

        return sum / count;
    }

    public float GenerateLayer(int x, int y, float positionX, float PositionZ, float BorderCurve, float NoiseScale, float NoiseOffsetX, float NoiseOffsetZ, float Frequency, int Octaves, float Roughness, float Lacunarity, Vector2 GenerationPercentRange, Vector2 CutPercentRange, bool IsReverse)
    {
        float xCoord = (float)x / terrainManager.ChunkResolution * NoiseScale + NoiseOffsetZ + (positionX / terrainManager.ChunkWidth * NoiseScale);
        float zCoord = (float)y / terrainManager.ChunkResolution * NoiseScale + NoiseOffsetX + (PositionZ / terrainManager.ChunkWidth * NoiseScale);
        float noiseX = Mathf.PerlinNoise((x + terrainManager.Seed + ((positionX / terrainManager.ChunkWidth) * terrainManager.ChunkResolution)) * BorderCurve, (y + terrainManager.Seed + ((PositionZ / terrainManager.ChunkWidth) * terrainManager.ChunkResolution)) * BorderCurve) * 50f;
        float noiseY = Mathf.PerlinNoise((x + 100 + terrainManager.Seed + ((positionX / terrainManager.ChunkWidth) * terrainManager.ChunkResolution)) * BorderCurve, (y + 100 + terrainManager.Seed + ((PositionZ / terrainManager.ChunkWidth) * terrainManager.ChunkResolution)) * BorderCurve) * 50f;
        Vector2 distortedPixel = new Vector2(xCoord, zCoord) + new Vector2(noiseX, noiseY);
        float pixel = PerlinNoiseGenerator.PerlinNoise(new(distortedPixel.x, distortedPixel.y), Frequency, Octaves, Roughness, Lacunarity, GenerationPercentRange, CutPercentRange, IsReverse);

        if (pixel >= GenerationPercentRange.y - 0.0001f)
        {
            pixel = GenerationPercentRange.y - 0.0001f;
        }
        else if (pixel <= 0f)
        {
            pixel = 0f;
        }
        return pixel;
    }

    public int ApplyBiome(float continentalness, float temperature, float humidity)
    {
        int indexOfBiome = 0;
        foreach (var biome in terrainManager.Biomes)
        {
            foreach (var setting in biome.BiomeSettings)
            {
                if (
                continentalness >= setting.Continentalness.x && continentalness < setting.Continentalness.y &&
                temperature >= setting.Temperature.x && temperature < setting.Temperature.y &&
                humidity >= setting.Humidity.x && humidity < setting.Humidity.y
                )
                {
                    return indexOfBiome;
                }
            }
            indexOfBiome++;
        }
        return 100;
    }

    public float GenerateHeight(int z, int x, float continentalness, float temperature, float humidity)
    {
        float result = 1f;
        float contX = terrainManager.HeightWidth.x;
        float contY = terrainManager.HeightWidth.y;
        float continent = (contY - contX) - Mathf.Clamp(((contY - contX) / 2 - Mathf.Abs(continentalness - (contX + contY) / 2)) * terrainManager.InteriorHeightMultiplicator, 0, contY - contX);
        float river = (contY - contX) - (((contY - contX) / 2 - Mathf.Abs(continentalness - (contX + contY) / 2)) * (continent == (contY - contX) ? terrainManager.ExteriorHeightMultiplicator : terrainManager.InteriorHeightMultiplicator));

        float xCoord = (float)x / terrainManager.ChunkResolution * terrainManager.HeightNoiseScale + terrainManager.HeightNoiseOffsetZ + (transform.position.z / terrainManager.ChunkWidth * terrainManager.HeightNoiseScale);
        float zCoord = (float)z / terrainManager.ChunkResolution * terrainManager.HeightNoiseScale + terrainManager.HeightNoiseOffsetX + (transform.position.x / terrainManager.ChunkWidth * terrainManager.HeightNoiseScale);
        float waves = PerlinNoiseGenerator.PerlinNoise(new Vector2(xCoord, zCoord), terrainManager.HeightFrequency, terrainManager.HeightOctaves, terrainManager.HeightRoughness, terrainManager.HeightLacunarity, terrainManager.HeightGenerationPercentRange, terrainManager.HeightCutPercentRange, terrainManager.HeightIsReverse);

        result *= (continent != (contY - contX) ? (1f + waves) * river : river) * (continent == (contY - contX) ? 1f + waves : 1f);
        return result;
    }
    #endregion

    #region |-- OTHER METHODS --|
    private void InitializeVariables()
    {
        terrain = GetComponent<Terrain>();
    }

    public void SetVariables(TerrainManager manager, bool forceVil)
    {
        terrainManager = manager;
        forceVillage = forceVil;
    }
   
    #endregion
}
