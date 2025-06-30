using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemySpawnerDesert : MonoBehaviour
{
    public TextMeshProUGUI wavePopupText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI levelCompleteText;
    public GameObject nextLevelButton;

    public float initialSpawnRate = 3f;
    public float minSpawnRate = 0.5f;
    public float spawnRateDecrease = 0.2f;
    public int enemiesPerWave = 10;
    public float popupDuration = 2f;

    public GameObject[] enemyPrefabs;

    private float spawnRate;
    private float timer;

    private int currentWave = 1;
    public int enemiesSpawnedThisWave = 0;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool levelComplete = false;

    void Start()
    {
        spawnRate = initialSpawnRate;
        timer = 0f;
        UpdateWaveText();

        wavePopupText.text = "";
        wavePopupText.alpha = 0f;

        if (levelCompleteText != null)
            levelCompleteText.gameObject.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);
    }

    void Update()
    {
        if (levelComplete) return;

        timer += Time.deltaTime;

        if (enemiesSpawnedThisWave >= enemiesPerWave * currentWave && aliveEnemies.Count == 0)
        {
            currentWave++;

            if (currentWave > 3)
            {
                HandleLevelComplete();
                return;
            }

            enemiesSpawnedThisWave = 0;
            spawnRate = Mathf.Max(minSpawnRate, spawnRate - spawnRateDecrease);
            UpdateWaveText();
            StartCoroutine(ShowWavePopup($"Wave {currentWave}"));
        }

        if (enemiesSpawnedThisWave < enemiesPerWave * currentWave && timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner has no prefabs assigned.");
            return;
        }

        GameObject selectedEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject newEnemy = Instantiate(selectedEnemy);

        enemiesSpawnedThisWave++;
        RegisterEnemySpawn(newEnemy);
    }

    public void RegisterEnemySpawn(GameObject enemy)
    {
        if (!aliveEnemies.Contains(enemy))
            aliveEnemies.Add(enemy);
    }

    public void UnregisterEnemyDeath(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);
    }

    void UpdateWaveText()
    {
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";
    }

    IEnumerator ShowWavePopup(string message)
    {
        if (wavePopupText == null) yield break;

        wavePopupText.text = message;

        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            wavePopupText.alpha = Mathf.Lerp(0, 1, t / 0.5f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(popupDuration);

        t = 0f;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            wavePopupText.alpha = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }

        wavePopupText.text = "";
    }

    void HandleLevelComplete()
    {
        levelComplete = true;

        if (levelCompleteText != null)
        {
            levelCompleteText.gameObject.SetActive(true);
            levelCompleteText.text = "All Targets Eliminated! Great Work!";
        }

        if (nextLevelButton != null)
            nextLevelButton.SetActive(true);
    }


    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}