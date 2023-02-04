using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class EnemyPlacer_Perlin : BaseEnemyPlacer
{
    [SerializeField] Vector2 NoiseScale = new Vector2(1f / 128f, 1 / 128f);
    [SerializeField] float NoiseThreshold = 0.5f;

    List<Vector3> GetFilteredLocationsForBiome(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap, int biomeIndex)
    {
        List<Vector3> locations = new List<Vector3>(mapResolution * mapResolution / 10);

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                if (biomeMap[x, y] != biomeIndex)
                    continue;
                
                // calcula o valor do noise
                float noiseValue = Mathf.PerlinNoise(x * NoiseScale.x, y * NoiseScale.y);

                // o noise tem de se encontrar acima do valor estipulado para ser considerado o ponto candidato
                if (noiseValue < NoiseThreshold)
                    continue;

                float height = heightMap[x, y] * heightmapScale.y;

                locations.Add(new Vector3(y * heightmapScale.z, height, x * heightmapScale.x));
            }
        }

        return locations;
    }

    public override void Execute(ProcGenConfigSO globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        base.Execute(globalConfig, objectRoot, mapResolution, heightMap, heightmapScale, slopeMap, alphaMaps, alphaMapResolution,
                     biomeMap, biomeIndex, biome);
        
        // arranjar uma potencial localização de spawn
        List<Vector3> candidateLocations = GetFilteredLocationsForBiome(globalConfig, mapResolution, heightMap, heightmapScale, biomeMap, biomeIndex);

        ExecuteSimpleSpawning(globalConfig, objectRoot, candidateLocations); 
    }
}
