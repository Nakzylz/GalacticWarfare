using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float flyInSpeed = 5f;
    public float flyAtPlayerSpeed = 10f;
    public float shootInterval = 1f;
    public int shotsToFire = 3;
    public float spawnSideOffset = 5f;
    public float stopDistanceFromPlayer = 3f;
    public float despawnDistanceBehindCamera = 5f;
    public float maxLifetime = 15f; 

    public GameObject projectilePrefab;
    public Transform shootPoint;

    private Transform player;
    private Camera mainCam;
    private Vector3 targetPositionInFront;

    private float lifetimeTimer = 0f;

    private EnemySpawner spawner;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCam = Camera.main;
        spawner = FindObjectOfType<EnemySpawner>();

        if (player == null || shootPoint == null || projectilePrefab == null || spawner == null)
        {
            Debug.LogWarning("EnemyController setup incomplete on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        // Calculate target fly-in position
        float side = Random.value > 0.5f ? 1f : -1f;
        transform.position = player.position + player.right * side * spawnSideOffset + player.forward * -5f;

        float randomOffset = Random.Range(-spawnSideOffset, spawnSideOffset);
        targetPositionInFront = player.position + player.forward * stopDistanceFromPlayer + player.right * randomOffset;

        // Face toward fly-in direction
        Vector3 initialLookDir = (targetPositionInFront - transform.position).normalized;
        initialLookDir.y = 0;
        transform.rotation = SafeLookRotation(initialLookDir);

        // Register this enemy with the spawner
        spawner.RegisterEnemySpawn(this.gameObject);

        StartCoroutine(StateMachine());
    }

    void Update()
    {
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= maxLifetime)
        {
            Despawn();
        }
    }

    IEnumerator StateMachine()
    {
        // --- Fly in ---
        while (Vector3.Distance(transform.position, targetPositionInFront) > 0.1f)
        {
            Vector3 dir = (targetPositionInFront - transform.position).normalized;
            dir.y = 0;
            transform.position += dir * flyInSpeed * Time.deltaTime;
            transform.rotation = SafeLookRotation(dir);
            yield return null;
        }

        // --- Face player once ---
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = SafeLookRotation(lookDir);

        // --- Shoot ---
        for (int i = 0; i < shotsToFire; i++)
        {
            Shoot();
            yield return new WaitForSeconds(shootInterval);
        }

        // --- Dive at player without tracking ---
        Vector3 diveDir = (player.position - transform.position).normalized;
        diveDir.y = 0;
        transform.rotation = SafeLookRotation(diveDir);

        while (Vector3.Distance(transform.position, player.position) > 0.5f)
        {
            transform.position += diveDir * flyAtPlayerSpeed * Time.deltaTime;
            yield return null;
        }

        // --- Exit behind camera ---
        Vector3 exitDir = -player.forward;
        while (true)
        {
            transform.position += exitDir * flyAtPlayerSpeed * Time.deltaTime;

            Vector3 toCam = transform.position - mainCam.transform.position;
            float dot = Vector3.Dot(toCam, mainCam.transform.forward);

            if (dot < -despawnDistanceBehindCamera)
            {
                Despawn();
                yield break;
            }

            yield return null;
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            Vector3 dir = (player.position - shootPoint.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir);
            Instantiate(projectilePrefab, shootPoint.position, rot);
        }
    }

    Quaternion SafeLookRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return Quaternion.identity;
        return Quaternion.LookRotation(direction);
    }

    void Despawn()
    {
        // Notify spawner that this enemy died/despawned
        if (spawner != null)
        {
            spawner.UnregisterEnemyDeath(this.gameObject);
        }
        Destroy(gameObject);
    }

    // Call this on enemy death or when it should be removed
    public void OnDeath()
    {
        Despawn();
    }
}












