using UnityEngine;
using System.Collections.Generic;

public class WorldScroller : MonoBehaviour
{
    public GameObject[] worldPrefabs;
    public float segmentLength = 50f;
    public float moveSpeed = 10f;
    public int segmentsAhead = 3;
    public Transform player;

    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float zSpawn = 0f;

    void Start()
    {
        for (int i = 0; i < segmentsAhead; i++)
            SpawnSegment();
    }

    void Update()
    {
        foreach (var segment in activeSegments)
            segment.transform.position -= Vector3.forward * moveSpeed * Time.deltaTime;

        if (activeSegments.Count > 0 && activeSegments.Peek().transform.position.z < player.position.z - segmentLength)
        {
            Destroy(activeSegments.Dequeue());
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        GameObject prefab = worldPrefabs[Random.Range(0, worldPrefabs.Length)];
        GameObject newSegment = Instantiate(prefab, new Vector3(0, 0, zSpawn), Quaternion.identity);
        activeSegments.Enqueue(newSegment);
        zSpawn += segmentLength;
    }
}

