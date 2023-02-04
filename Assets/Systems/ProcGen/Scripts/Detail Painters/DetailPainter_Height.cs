using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailPainter_Height : BaseDetailPainter
{
    [SerializeField] TerrainDetailConfig TerrainDetail;
    [SerializeField] float StartHeight;
    [SerializeField] float EndHeight;
    [SerializeField] AnimationCurve Intensity;
    [SerializeField] bool SuppressOtherDetails = false;
    [SerializeField] AnimationCurve SuppressionIntensity;

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, List<int[,]> detailLayerMaps, int detailMapResolution, int maxDetailsPerPatch, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int detailLayer = manager.GetDetailLayerForTerrainDetail(TerrainDetail);

        float heightMapStart = StartHeight / heightmapScale.y;
        float heightMapEnd = EndHeight / heightmapScale.y;
        float heightMapRangeInv = 1f / (heightMapEnd - heightMapStart);

        int numDetailLayers = detailLayerMaps.Count;

        for (int y = 0; y < detailMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)detailMapResolution);
            
            for (int x = 0; x < detailMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)detailMapResolution);

                // passar se tivermos um bioma mas este não ser o nosso bioma
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                float height = heightMap[heightMapX, heightMapY];

                // fora do alcance de altura
                if (height < heightMapStart || height > heightMapEnd)
                    continue;

                float heightPercentage = (height - heightMapStart) * heightMapRangeInv;
                detailLayerMaps[detailLayer][x, y] = Mathf.FloorToInt(Strength * Intensity.Evaluate(heightPercentage) * maxDetailsPerPatch);

                // se a supressão de outros detalhes estiver ativada então faz a atualização das outras camadas
                if (SuppressOtherDetails)
                {
                    float suppression = SuppressionIntensity.Evaluate(heightPercentage);

                    // aplicar a supressão noutras camadas
                    for (int layerIndex = 0; layerIndex < numDetailLayers; ++layerIndex)
                    {
                        if (layerIndex == detailLayer)
                            continue;

                        detailLayerMaps[detailLayer][x, y] = Mathf.FloorToInt(detailLayerMaps[detailLayer][x, y] * suppression);
                    }
                }
            }
        }
    }

    public override List<TerrainDetailConfig> RetrieveTerrainDetails()
    {
        List<TerrainDetailConfig> allTerrainDetails = new List<TerrainDetailConfig>(1);
        allTerrainDetails.Add(TerrainDetail);

        return allTerrainDetails;
    }
}