using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace KhiemLuong
{
    public class NPC_Spawner : MonoBehaviour
    {
        [SerializeField] GameObject[] npcPrefabs;
        [SerializeField] NavMeshSurface navMeshSurface;
        private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>(); // Hash set to track used spawn points

        void Start()
        {
            // Collect all child GameObjects as spawn points
            List<Transform> spawnPoints = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                spawnPoints.Add(transform.GetChild(i));
            }

            // Shuffle the list of spawn points to randomize order
            int n = spawnPoints.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                Transform value = spawnPoints[k];
                spawnPoints[k] = spawnPoints[n];
                spawnPoints[n] = value;
            }

            // Spawn each NPC prefab at a random spawn point from the collected spawn points
            foreach (var prefab in npcPrefabs)
            {
                foreach (var spawnPoint in spawnPoints)
                {
                    // Check if spawn point has already been used
                    if (!usedSpawnPoints.Contains(spawnPoint))
                    {
                        // Instantiate the prefab at the spawn point
                        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

                        // Add the spawn point to the used set to avoid reusing it
                        usedSpawnPoints.Add(spawnPoint);
                        break; // Break out of the inner loop once a spawn point is used
                    }
                }
            }
        }
    }
}