using UnityEngine;
using System.Collections.Generic;

public class WorldChunkSpawner : MonoBehaviour
{
    public GameObject[] chunkPrefabs;
    public int chunksAhead = 3;
    public float verticalOffset = -40f;
    public Transform player;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float chunkLength = 50f; // fallback

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
            GameObject firstChunk = activeChunks[0];
            float playerZ = player.position.z;

            // If chunk is far behind the player
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
        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];

        // Determine new Z position
        float newZ = 0f;

        if (activeChunks.Count > 0)
        {
            GameObject lastChunk = activeChunks[activeChunks.Count - 1];

            // Optional: Automatically determine actual chunk length
            float lastChunkLength = chunkLength;
            Renderer rend = lastChunk.GetComponentInChildren<Renderer>();
            if (rend != null)
                lastChunkLength = rend.bounds.size.z;

            newZ = lastChunk.transform.position.z + lastChunkLength;
        }

        Vector3 spawnPos = new Vector3(0, verticalOffset, newZ);
        GameObject newChunk = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeChunks.Add(newChunk);

        // Optional: update chunkLength based on this chunk’s size (if variable length)
        Renderer thisRenderer = newChunk.GetComponentInChildren<Renderer>();
        if (thisRenderer != null)
            chunkLength = thisRenderer.bounds.size.z;
    }
}




