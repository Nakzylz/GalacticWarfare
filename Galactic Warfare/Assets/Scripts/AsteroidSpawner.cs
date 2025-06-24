using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;
    public float spawnRate = 1.5f;
    public float forwardSpawnDistance = 30f;
    public float horizontalRange = 10f;
    public float verticalRange = 5f;
    public float asteroidSpeed = 10f;

    private float timer;

    void Start()
    {
        timer = 0f; // Reset timer on scene load
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnAsteroid();
            timer = 0f;
        }
    }

    void SpawnAsteroid()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0) return;

        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * forwardSpawnDistance;
        spawnPos += new Vector3(Random.Range(-horizontalRange, horizontalRange), Random.Range(-verticalRange, verticalRange), 0f);

        GameObject asteroid = Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)], spawnPos, Quaternion.identity);

        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = asteroid.AddComponent<Rigidbody>();
        }
        rb.useGravity = false; // Prevent falling
        rb.velocity = -Camera.main.transform.forward * asteroidSpeed;
    }
}


