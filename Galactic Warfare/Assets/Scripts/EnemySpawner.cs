using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class EnemySpawner : MonoBehaviour
{
    public Animator transition;

    public Canvas gameplayCanvas; // Assign your HUD Canvas here
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    public float transitionTime = 1f;

    public TextMeshProUGUI wavePopupText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI levelCompleteText;
    public GameObject nextLevelButton;

    public GameObject[] enemyPrefabs;
    public float initialSpawnRate = 3f;
    public float minSpawnRate = 0.5f;
    public float spawnRateDecrease = 0.2f;
    public int enemiesPerWave = 10;
    public float popupDuration = 2f;
    public int maxActiveEnemies = 10;

    public float formationSpacing = 5f;
    public float stopDistanceFromPlayer = 3f;

    private float spawnRate;
    private float timer;
    private int currentWave = 1;
    private int enemiesSpawnedThisWave = 0;


    private List<GameObject> aliveEnemies = new List<GameObject>();
    private List<Vector3> formationOffsets = new List<Vector3>();
    private List<Vector3> reservedPositions = new List<Vector3>();

    private bool levelComplete = false;
    private Transform player;

    void Start()
    {
        Time.timeScale = 1.0f;
        SetupCanvasScaler(gameplayCanvas);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        spawnRate = initialSpawnRate;
        timer = 0f;
        UpdateWaveText();
        wavePopupText.text = "";
        wavePopupText.alpha = 0f;

        if (levelCompleteText != null) levelCompleteText.gameObject.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);

        InitializeFormationSlots();
    }

    void SetupCanvasScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }


    void Update()
    {
        if (levelComplete || player == null) return;

        timer += Time.deltaTime;

        if (enemiesSpawnedThisWave >= enemiesPerWave * currentWave && aliveEnemies.Count == 0)
        {
            currentWave++;

            if (currentWave > 6)
            {
                HandleLevelComplete();
                return;
            }

            enemiesSpawnedThisWave = 0;
            spawnRate = Mathf.Max(minSpawnRate, spawnRate - spawnRateDecrease);
            UpdateWaveText();
            StartCoroutine(ShowWavePopup($"Wave {currentWave}"));
        }

        if (enemiesSpawnedThisWave < enemiesPerWave * currentWave && timer >= spawnRate && aliveEnemies.Count < maxActiveEnemies)
        {
            TrySpawnEnemy();
            timer = 0f;
        }
    }

    void InitializeFormationSlots()
    {
        formationOffsets.Clear();
        formationOffsets.Add(Vector3.zero); // center

        for (int i = 1; i < maxActiveEnemies; i++)
        {
            float offset = Mathf.Ceil(i / 2f) * formationSpacing;
            Vector3 sideOffset = (i % 2 == 0) ? Vector3.right * offset : Vector3.left * offset;
            formationOffsets.Add(sideOffset);
        }
    }


    void TrySpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        Vector3 basePosition = player.position + player.forward * stopDistanceFromPlayer;

        foreach (Vector3 offset in formationOffsets)
        {
            Vector3 worldPos = basePosition + player.right * offset.x;

            bool alreadyUsed = false;
            foreach (Vector3 reserved in reservedPositions)
            {
                if (Vector3.Distance(reserved, worldPos) < formationSpacing * 1f)
                {
                    alreadyUsed = true;
                    break;
                }
            }

            if (!alreadyUsed)
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject newEnemy = Instantiate(prefab);

                if (newEnemy.TryGetComponent(out EnemyController ec))
                {
                    ec.SetTargetFormation(worldPos, this);
                }

                reservedPositions.Add(worldPos);
                RegisterEnemySpawn(newEnemy);
                enemiesSpawnedThisWave++;
                return;
            }
        }
    }

    public void RegisterEnemySpawn(GameObject enemy)
    {
        if (!aliveEnemies.Contains(enemy))
            aliveEnemies.Add(enemy);
    }

    public void UnregisterEnemyDeath(GameObject enemy, Vector3 releasedPosition)
    {
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);

        reservedPositions.Remove(releasedPosition);
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
        Time.timeScale = 0.3f;

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
        StartCoroutine(LoadStart());
    }

    IEnumerator LoadStart()
    {
        transition.SetTrigger("Fade");

        yield return new WaitForSeconds(transitionTime);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 2);
    }
}












