using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ruins : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] List<Transform> SpawnPoints;
    [SerializeField] int InitialPopulation;

    // Start is called before the first frame update
    void Start()
    {
        List<Transform> workingSpawnPoints = new List<Transform>(SpawnPoints);

        // spawnar a população inicial de inimigos
        for (int index = 0; index < InitialPopulation; ++index)
        {
            // escolher um local de spawn
            int spawnIndex = Random.Range(0, workingSpawnPoints.Count);
            Transform spawnPoint = workingSpawnPoints[spawnIndex];
            workingSpawnPoints.RemoveAt(spawnIndex);

            // spawnar o inimigo
            var enemy = Instantiate(EnemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
