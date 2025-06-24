using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public Transform[] obstacleSpawnPoints;

    void Start()
    {
        foreach (Transform point in obstacleSpawnPoints)
        {
            if (Random.value < 0.5f) // 50% chance to spawn
            {
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                Instantiate(prefab, point.position, Quaternion.identity, transform);
            }
        }
    }
}

