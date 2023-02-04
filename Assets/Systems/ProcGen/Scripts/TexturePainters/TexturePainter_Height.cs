using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter_Height : BaseTexturePainter
{
    [SerializeField] TextureConfig Texture;
    [SerializeField] float StartHeight;
    [SerializeField] float EndHeight;
    [SerializeField] AnimationCurve Intensity;
    [SerializeField] bool SuppressOtherTextures = false;
    [SerializeField] AnimationCurve SuppressionIntensity;

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int textureLayer = manager.GetLayerForTexture(Texture);

        float heightMapStart = StartHeight / heightmapScale.y;
        float heightMapEnd = EndHeight / heightmapScale.y;
        float heightMapRangeInv = 1f / (heightMapEnd - heightMapStart);

        int numAlphaMaps = alphaMaps.GetLength(2);

        for (int y = 0; y < alphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)alphaMapResolution);
            for (int x = 0; x < alphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)alphaMapResolution);

                // passar se tivermos um bioma e isto não ser o nosso bioma
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                // fora do range de alturas
                float height = heightMap[heightMapX, heightMapY];
                if (height < heightMapStart || height > heightMapEnd)
                    continue;

                float heightPercentage = (height - heightMapStart) * heightMapRangeInv;
                alphaMaps[x, y, textureLayer] = Strength * Intensity.Evaluate(heightPercentage);

                // se a supressão de outras texturas estiver ligada então faz a atualização das outras camadas
                if (SuppressOtherTextures)
                {
                    float supression = SuppressionIntensity.Evaluate(heightPercentage);

                    // aplicar a supressão para as outras camadas
                    for (int layerIndex = 0; layerIndex < numAlphaMaps; ++layerIndex)
                    {
                        if (layerIndex == textureLayer)
                            continue;

                        alphaMaps[x, y, layerIndex] *= supression;
                    }
                }
            }
        }
    }

    public override List<TextureConfig> RetrieveTextures()
    {
        List<TextureConfig> allTextures = new List<TextureConfig>(1);
        allTextures.Add(Texture);

        return allTextures;
    }
}