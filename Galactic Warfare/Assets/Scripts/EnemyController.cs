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

    public GameObject projectilePrefab;
    public Transform shootPoint;

    private Transform player;
    private Camera mainCam;
    private Vector3 targetPositionInFront;
    private Quaternion fixedRotation; // Stores locked rotation for diving and exit phases

    IEnumerator Start()
    {
        // Delay to ensure player and camera are initialized
        yield return null;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCam = Camera.main;

        if (player == null || shootPoint == null || projectilePrefab == null)
        {
            Debug.LogWarning("EnemyController missing references on " + name);
            Destroy(gameObject);
            yield break;
        }

        // Set spawn position to one side behind the player
        float side = Random.value > 0.5f ? 1f : -1f;
        transform.position = player.position + player.right * side * spawnSideOffset + player.forward * -5f;

        // Compute fly-in target
        float randX = Random.Range(-spawnSideOffset, spawnSideOffset);
        targetPositionInFront = player.position + player.forward * stopDistanceFromPlayer + player.right * randX;

        // Rotate toward fly-in direction
        Vector3 toTarget = targetPositionInFront - transform.position;
        fixedRotation = SafeLook(toTarget);
        transform.rotation = fixedRotation;

        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        // --- Fly in with active rotation update ---
        while (Vector3.Distance(transform.position, targetPositionInFront) > 0.1f)
        {
            Vector3 dir = targetPositionInFront - transform.position;
            transform.position += dir.normalized * flyInSpeed * Time.deltaTime;
            transform.rotation = SafeLook(dir);
            yield return null;
        }

        // --- Lock rotation toward player before shooting ---
        Vector3 toPlayer = player.position - transform.position;
        fixedRotation = SafeLook(toPlayer);
        transform.rotation = fixedRotation;

        // --- Shooting phase ---
        for (int i = 0; i < shotsToFire; i++)
        {
            Shoot();
            yield return new WaitForSeconds(shootInterval);
        }

        // --- Diving: move forward in locked forward direction, no further rotation ---
        while (Vector3.Distance(transform.position, player.position) > 0.5f)
        {
            transform.position += transform.forward * flyAtPlayerSpeed * Time.deltaTime;
            yield return null;
        }

        // --- Exit: move backward, no rotation changes ---
        Vector3 exitDir = -player.forward.normalized;
        while (true)
        {
            transform.position += exitDir * flyAtPlayerSpeed * Time.deltaTime;

            Vector3 toCam = transform.position - mainCam.transform.position;
            float dot = Vector3.Dot(toCam, mainCam.transform.forward);
            if (dot < -despawnDistanceBehindCamera)
            {
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    void Shoot()
    {
        Vector3 dir = (player.position - shootPoint.position);
        Quaternion rot = SafeLook(dir);
        Instantiate(projectilePrefab, shootPoint.position, rot);
    }

    // Returns a safe rotation toward `dir`, or current rotation if invalid
    private Quaternion SafeLook(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f || float.IsNaN(dir.x) || float.IsNaN(dir.z))
        {
            return transform.rotation; // Keep current facing
        }
        return Quaternion.LookRotation(dir.normalized);
    }
}









