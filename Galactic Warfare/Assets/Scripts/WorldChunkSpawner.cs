using UnityEngine;
using System.Collections.Generic;

public class WorldChunkSpawner : MonoBehaviour
{
    public float chunkLength = 50f;   // Distance between chunks
    public int chunksAhead = 3;       // How many chunks to keep ahead
    public GameObject[] chunkPrefabs; // Prefabs of different world chunks
    public Transform player;          // The stationary player

    private float spawnZ = 0f;
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < chunksAhead; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        if (activeChunks.Count > 0)
        {
            float playerZ = player.position.z;
            GameObject firstChunk = activeChunks[0];

            if (firstChunk.transform.position.z + chunkLength < playerZ - 20f)
            {
                Destroy(firstChunk);
                activeChunks.RemoveAt(0);
                SpawnChunk();
            }
        }
    }

    void SpawnChunk()
    {
        float verticalOffset = -40f; // You can adjust this value as needed
        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        GameObject chunk = Instantiate(prefab, new Vector3(0, verticalOffset, spawnZ), Quaternion.identity);
        activeChunks.Add(chunk);
        spawnZ += chunkLength;
    }

}

