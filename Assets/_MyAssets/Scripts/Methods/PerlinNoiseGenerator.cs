using UnityEngine;

public static class PerlinNoiseGenerator
{
    public static float PerlinNoise(Vector2 coordinates, float frequency = 1f, int octaves = 1, float roughness = 0.5f, float lacunarity = 2f, Vector2 generationRange = default, Vector2 cutRange = default, bool isReverse = false)
    {
        // Ensure range has a valid default
        if (generationRange == default)
        {
            generationRange = new Vector2(0f, 1f);
        }

        if (cutRange == default)
        {
            cutRange = new Vector2(0f, 1f);
        }

        float noise = 0f;
        float amplitude = 1f; // Initial amplitude
        float maxAmplitude = 0f; // Normalization factor
        Vector2 adjustedCoord = coordinates * frequency;

        for (int i = 0; i < octaves; i++)
        {
            // Generate Perlin noise for the current octave
            noise += Mathf.PerlinNoise(adjustedCoord.x, adjustedCoord.y) * amplitude;
            maxAmplitude += amplitude;

            // Scale coordinate and amplitude for the next octave
            adjustedCoord *= lacunarity;
            amplitude *= roughness;
        }

        // Normalize the noise to [0, 1]
        noise /= maxAmplitude;

        if(noise > cutRange.y)
        {
            noise = cutRange.y;
        }
        else if(noise < cutRange.x)
        {
            noise = cutRange.x;
        }

        // Put the noise at the lowest height possible
        noise -= cutRange.x;

        // Reverse the noise if needed
        if (isReverse)
        {
            noise = cutRange.y - cutRange.x - noise;
        }


        // Scale the noise to the specified range
        noise = Mathf.Lerp(generationRange.x, generationRange.y, noise);


        return noise;
    }
}
