using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 100f;
    public float deathDelay = 2f; // Time before destruction
    public ParticleSystem deathEffect; // Assign in Inspector

    private bool isDead = false;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            health -= 100;
        }
        if (other.gameObject.CompareTag("Asteroid"))
        {
            health -= 100;
        }
        if (other.gameObject.CompareTag("DesertRocks"))
        {
            health -= 100;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            health -= 50;
        }
    }

    void Update()
    {
        if (!isDead && health <= 0)
        {
            isDead = true;
            StartCoroutine(HandleEnemyDeath());
        }
    }

    IEnumerator HandleEnemyDeath()
    {
        // Disable the MeshRenderer on this GameObject (if any)
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        // Disable all children GameObjects
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // Instantiate and play particle effect prefab at enemy's position
        if (deathEffect != null)
        {
            ParticleSystem effectInstance = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effectInstance.Play();
        }

        // Notify spawner that this enemy is dead
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.UnregisterEnemyDeath(gameObject);
        }

        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }
}


