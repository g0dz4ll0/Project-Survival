using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapModifier_Offset : BaseHeightMapModifier
{
    [SerializeField] float OffsetAmount;

    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                // passar se tivermos um bioma e isto não ser o nosso bioma
                if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                    continue;

                // calcula a nova altura
                float newHeight = heightMap[x, y] + (OffsetAmount / heightmapScale.y);

                // misturar baseado na força
                heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, Strength);
            }
        }
    }
}
