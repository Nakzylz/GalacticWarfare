using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 100f;
    public float deathDelay = 2f; // Time before destruction
    public ParticleSystem deathEffect; // Assign in Inspector

    [Header("Audio")]
    public AudioClip explosionSound;
    private AudioSource audioSource;


    private bool isDead = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


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
            Destroy(other.gameObject);
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
        // Play explosion sound BEFORE disabling things
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Disable MeshRenderer
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        // Disable all children
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);

        // Play particle
        if (deathEffect != null)
        {
            ParticleSystem effectInstance = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effectInstance.Play();
        }

        // Notify controller to handle despawn
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.OnDeath();
        }

        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }


}


