using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapModifier_Smooth : BaseHeightMapModifier
{
    [SerializeField] int SmoothingKernelSize = 5;

    [SerializeField] bool UseAdaptiveKernel = false;
    [SerializeField] [Range(0f, 1f)] float MaxHeightThreshold = 0.5f;
    [SerializeField] int MinKernelSize = 2;
    [SerializeField] int MaxKernelSize = 7;

    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        if (biomeMap != null)
        {
            Debug.LogError("HeightMapModifier_Smooth não suportada um modificador para cada bioma [" + gameObject.name + "]");
            return;
        }

        float[,] smoothedHeights = new float[mapResolution, mapResolution];

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                float heightSum = 0f;
                int numValues = 0;

                // definir o tamanho do centro
                int kernelSize = SmoothingKernelSize;
                if (UseAdaptiveKernel)
                {
                    kernelSize = Mathf.RoundToInt(Mathf.Lerp(MaxKernelSize, MinKernelSize, heightMap[x, y] / MaxHeightThreshold));
                }

                // soma os valores vizinhos
                for (int yDelta = -kernelSize; yDelta <= kernelSize; ++yDelta)
                {
                    int workingY = y + yDelta;
                    if (workingY < 0 || workingY >= mapResolution)
                        continue;

                    for (int xDelta = -kernelSize; xDelta <= kernelSize; ++xDelta)
                    {
                        int workingX = x + xDelta;
                        if (workingX < 0 || workingX >= mapResolution)
                            continue;
                        
                        heightSum += heightMap[workingX, workingY];
                        ++numValues;
                    }
                }

                // guarda a altura suavizada (aka média)
                smoothedHeights[x, y] = heightSum / numValues;
            }
        }

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                // misturar baseado na força
                heightMap[x, y] = Mathf.Lerp(heightMap[x, y], smoothedHeights[x, y], Strength);
            }
        }
    }
}
