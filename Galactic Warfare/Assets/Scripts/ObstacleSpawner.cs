using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public int obstacleCount = 3;
    public float xRange = 8f;
    public float yRange = 5f;

    void Start()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 spawnPos = transform.position +
                               new Vector3(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange), Random.Range(-5f, 5f));
            Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], spawnPos, Quaternion.identity, transform);
        }
    }
}

