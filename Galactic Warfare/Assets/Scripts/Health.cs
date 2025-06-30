using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public float health = 100f;
    public float deathDelay = 2f; // Time before loading scene
    public ParticleSystem deathEffect; // Assign in Inspector
    public Slider healthUI;
    public Image fillImage;
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;
    public float warningThreshold = 0.3f;

    private bool isDead = false;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            health -= 50;
        }
        if (other.gameObject.CompareTag("Projectile"))
        {
            health -= 10;
        }
        if (other.gameObject.CompareTag("Asteroid"))
        {
            health -= 30;
        }
        if (other.gameObject.CompareTag("DesertRocks"))
        {
            health -= 500;
        }
    }

    void Update()
    {
        if (!isDead && health <= 0)
        {
            isDead = true;
            StartCoroutine(HandleDeath());
        }
        healthUI.value = health;
        float percent = healthUI.value / healthUI.maxValue;

        if (percent < warningThreshold)
        {
            fillImage.color = warningColor;
        }
        else
        {
            fillImage.color = normalColor;
        }
    }

    IEnumerator HandleDeath()
    {
        // Disable all other scripts on this GameObject
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

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

        // Instantiate and play particle effect prefab at player's position
        if (deathEffect != null)
        {
            ParticleSystem effectInstance = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effectInstance.Play();
        }

        yield return new WaitForSeconds(deathDelay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }


}

