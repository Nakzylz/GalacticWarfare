using System.Collections;
using UnityEngine;

public class AsteroidMover : MonoBehaviour
{
    public float speed = 10f;
    public ParticleSystem deathEffect;

    private Vector3 rotationAxis;
    private float rotationSpeed;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }

        // Set a random rotation axis and speed
        rotationAxis = Random.onUnitSphere; // Random direction in 3D space
        rotationSpeed = Random.Range(10f, 30f); // degrees per second
    }

    void Update()
    {
        // Move forward
        transform.position -= Vector3.forward * speed * Time.deltaTime;

        // Apply rotation
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);

        // Destroy if off screen
        if (transform.position.z < -20f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(HandleAsteroidDestruction());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerProjectile") || other.gameObject.CompareTag("Projectile"))
        {
            StartCoroutine(HandleAsteroidDestruction());
        }
    }

    IEnumerator HandleAsteroidDestruction()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        if (deathEffect != null)
        {
            ParticleSystem effectInstance = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effectInstance.Play();
        }

        yield return new WaitForSeconds(0.7f);

        Destroy(gameObject);
    }
}



