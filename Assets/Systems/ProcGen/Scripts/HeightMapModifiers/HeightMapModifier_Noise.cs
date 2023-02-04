using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeightNoisePass
{
    public float HeightDelta = 1f;
    public float NoiseScale = 1f;
}

public class HeightMapModifier_Noise : BaseHeightMapModifier
{
    [SerializeField] List<HeightNoisePass> Passes;

    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        foreach(var pass in Passes)
        {
            for (int y = 0; y < mapResolution; ++y)
            {
                for (int x = 0; x < mapResolution; ++x)
                {
                    // passar se tivermos um bioma e isto não ser o nosso bioma
                    if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                        continue;

                    float noiseValue = (Mathf.PerlinNoise(x * pass.NoiseScale, y * pass.NoiseScale) * 2f) - 1f;

                    // calcula a nova altura
                    float newHeight = heightMap[x, y] + (noiseValue * pass.HeightDelta / heightmapScale.y);

                    // misturar baseado na força
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, Strength);
                }
            }
        }
    }
}
