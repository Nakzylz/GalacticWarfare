using System.Collections.Generic;
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
    public float rotationSpeed = 5f;

    [Header("Audio")]
    public AudioClip shootSound;
    private AudioSource audioSource;

    public GameObject projectilePrefab;
    public Transform shootPoint;

    private Transform player;
    private Camera mainCam;
    private Vector3 targetPositionInFront;
    private float lifetimeTimer = 0f;
    private EnemySpawner spawner;
    private Vector3 formationTarget;
    private EnemySpawner assignedSpawner;
    private static List<Vector3> reservedTargetPositions = new List<Vector3>();


    private bool isDiving = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCam = Camera.main;
        spawner = FindObjectOfType<EnemySpawner>();

        if (player == null || shootPoint == null || projectilePrefab == null || spawner == null)
        {
            Debug.LogWarning("EnemyController setup incomplete on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        // Initial spawn position (off-screen side)
        float side = Random.value > 0.5f ? 1f : -1f;
        transform.position = player.position + player.right * side * spawnSideOffset + player.forward * -15f;

        // Attempt to pick a non-overlapping target position
        int maxAttempts = 10;
        float spacing = 10f;
        bool foundValid = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            float offset = Random.Range(-spawnSideOffset, spawnSideOffset);
            Vector3 potentialTarget = player.position + player.forward * stopDistanceFromPlayer + player.right * offset;

            bool overlap = false;
            foreach (Vector3 reserved in reservedTargetPositions)
            {
                if (Vector3.Distance(reserved, potentialTarget) < spacing)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                reservedTargetPositions.Add(potentialTarget);
                targetPositionInFront = potentialTarget;
                foundValid = true;
                break;
            }
        }

        if (!foundValid)
        {
            // fallback to random position if none found
            float fallbackOffset = Random.Range(-spawnSideOffset, spawnSideOffset);
            targetPositionInFront = player.position + player.forward * stopDistanceFromPlayer + player.right * fallbackOffset;
            reservedTargetPositions.Add(targetPositionInFront);
        }

        // Face toward fly-in direction
        Vector3 initialLookDir = (targetPositionInFront - transform.position).normalized;
        initialLookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(initialLookDir);

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

        if (!isDiving && player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    public void SetTargetFormation(Vector3 target, EnemySpawner spawner)
    {
        formationTarget = target;
        assignedSpawner = spawner;
    }


    IEnumerator StateMachine()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = targetPositionInFront;

        Vector2 start2D = new Vector2(startPos.x, startPos.z);
        Vector2 end2D = new Vector2(endPos.x, endPos.z);

        Vector2 midPoint = (start2D + end2D) / 2f;
        Vector2 dir = (end2D - start2D).normalized;
        Vector2 perp = new Vector2(dir.y, -dir.x);

        float chordLength = Vector2.Distance(start2D, end2D);
        float radius = Mathf.Max(5f, chordLength);
        float h = Mathf.Sqrt(radius * radius - (chordLength * chordLength) / 4f);
        float sideSign = Mathf.Sign(Vector3.Dot(transform.position - player.position, player.right));
        Vector2 center2D = midPoint + perp * h * sideSign;

        Vector2 startRel = start2D - center2D;
        Vector2 endRel = end2D - center2D;

        float startAngle = Mathf.Atan2(startRel.y, startRel.x);
        float endAngle = Mathf.Atan2(endRel.y, endRel.x);

        if (sideSign < 0 && endAngle > startAngle)
            endAngle -= 2 * Mathf.PI;
        else if (sideSign > 0 && endAngle < startAngle)
            endAngle += 2 * Mathf.PI;

        float arcLength = Mathf.Abs(endAngle - startAngle);
        float flyDistance = radius * arcLength;
        float progress = 0f;

        Vector3 avoidanceOffset = Vector3.zero;

        while (progress < 1f)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, progress);
            Vector2 pos2D = new Vector2(
                center2D.x + Mathf.Cos(angle) * radius,
                center2D.y + Mathf.Sin(angle) * radius
            );

            Vector3 newPos = new Vector3(pos2D.x, startPos.y, pos2D.y);

            // Calculate direction toward new position
            Vector3 moveDir = (newPos - transform.position).normalized;
            float currentSpeed = flyInSpeed;

            // Obstacle detection
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(rayOrigin, moveDir, out hit, 3f))
            {
                if (hit.collider.CompareTag("DesertRocks"))
                {
                    Vector3 dodgeDir = Vector3.Cross(Vector3.up, moveDir).normalized;
                    float side = (Random.value > 0.5f) ? 1f : -1f;
                    avoidanceOffset += dodgeDir * 1.5f * side;

                    currentSpeed *= 0.5f; // slow down during avoidance
                }
            }

            // Smooth out avoidance
            avoidanceOffset = Vector3.Lerp(avoidanceOffset, Vector3.zero, Time.deltaTime * 0.5f);
            newPos += avoidanceOffset;

            // Apply position
            transform.position = newPos;

            // Roll and direction
            float tangentAngle = angle + (sideSign > 0 ? Mathf.PI / 2f : -Mathf.PI / 2f);
            Vector3 tangentDir = new Vector3(Mathf.Cos(tangentAngle), 0, Mathf.Sin(tangentAngle)).normalized;

            float maxBank = 30f;
            float bank = Mathf.Sin(progress * Mathf.PI) * maxBank * sideSign;

            Quaternion rot = Quaternion.LookRotation(tangentDir);
            Vector3 euler = rot.eulerAngles;
            euler.z = bank;
            transform.rotation = Quaternion.Euler(euler);

            progress += (currentSpeed / flyDistance) * Time.deltaTime;
            progress = Mathf.Clamp01(progress);

            yield return null;
        }

        // --- Smooth turn to face player ---
        float turnDuration = 0.5f;
        float elapsed = 0f;

        Vector3 startForward = transform.forward;
        Vector3 targetDir = (player.position - transform.position);
        targetDir.y = 0;
        targetDir.Normalize();

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / turnDuration);

            Vector3 newForward = Vector3.Slerp(startForward, targetDir, t);
            Quaternion targetRotation = Quaternion.LookRotation(newForward);

            Vector3 euler = targetRotation.eulerAngles;
            euler.z = 0;
            transform.rotation = Quaternion.Euler(euler);

            yield return null;
        }

        // --- Shoot ---
        for (int i = 0; i < shotsToFire; i++)
        {
            Shoot();
            yield return new WaitForSeconds(shootInterval);
        }

        // --- Dive ---
        isDiving = true;
        Vector3 diveDir = (player.position - transform.position).normalized;
        diveDir.y = 0;
        transform.rotation = Quaternion.LookRotation(diveDir);

        while (Vector3.Distance(transform.position, player.position) > 0.5f)
        {
            transform.position += diveDir * flyAtPlayerSpeed * Time.deltaTime;
            yield return null;
        }

        // --- Exit ---
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

            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    void Despawn()
    {
        if (assignedSpawner != null)
            assignedSpawner.UnregisterEnemyDeath(this.gameObject, formationTarget);

        Destroy(gameObject);
    }



    public void OnDeath()
    {
        Despawn();
    }
}















