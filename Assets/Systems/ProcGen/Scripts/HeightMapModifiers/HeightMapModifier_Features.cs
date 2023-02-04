using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FeatureConfig
{
    public Texture2D HeightMap;
    public float Height;
    public int Radius;
    public int NumToSpawn = 1;
}

public class HeightMapModifier_Features : BaseHeightMapModifier
{
    [SerializeField] List<FeatureConfig> Features;

    protected void SpawnFeature(ProcGenConfigSO globalConfig, FeatureConfig feature, int spawnX, int spawnY,
                                int mapResolution, float[,] heightMap, Vector3 heightmapScale)
    {
        float averageHeight = 0f;
        int numHeightSamples = 0;

        // soma os valores de altura baseado nas características
        for (int y = -feature.Radius; y <= feature.Radius; ++y)
        {
            for (int x = -feature.Radius; x <= feature.Radius; ++x)
            {
                // soma os valores do heightmap
                averageHeight += heightMap[x + spawnX, y + spawnY];
                ++numHeightSamples;  
            }
        }

        // calcula a média de alturas
        averageHeight /= numHeightSamples;

        float targetHeight = averageHeight + (feature.Height / heightmapScale.y);

        // aplicar a característica
        for (int y = -feature.Radius; y <= feature.Radius; ++y)
        {
            int workingY = y + spawnY;
            float textureY = Mathf.Clamp01((float)(y + feature.Radius) / (feature.Radius * 2f));
            for (int x = -feature.Radius; x <= feature.Radius; ++x)
            {
                int workingX = x + spawnX;
                float textureX = Mathf.Clamp01((float)(x + feature.Radius) / (feature.Radius * 2f));

                // fazer uma amostra do heightmap
                var pixelColour = feature.HeightMap.GetPixelBilinear(textureX, textureY);
                float strength = pixelColour.r;

                // misturar baseado na força
                heightMap[workingX, workingY] = Mathf.Lerp(heightMap[workingX, workingY], targetHeight, strength);
            }
        }
    }
    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        // percorre as características
        foreach(var feature in Features)
        {
            for (int featureIndex = 0; featureIndex < feature.NumToSpawn; ++featureIndex)
            {
                int spawnX = Random.Range(feature.Radius, mapResolution - feature.Radius);
                int spawnY = Random.Range(feature.Radius, mapResolution - feature.Radius);

                SpawnFeature(globalConfig, feature, spawnX, spawnY, mapResolution, heightMap, heightmapScale);
            }
        }
    }
}
