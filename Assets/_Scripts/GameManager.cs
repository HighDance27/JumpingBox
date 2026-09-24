using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Difficulty Progression")]
    public DifficultyProgressionData difficultyData;

    [Header("Biome Configuration")]
    public BiomeData[] availableBiomes; // Order by scoreThreshold
    public Renderer groundRenderer;
    private int currentBiomeIndex = 0;
    private Coroutine groundTransitionCoroutine;

    [Header("Obstacle ScriptableObjects")]
    private ObstacleData[] obstacleTypes;
    public Transform spawnPoint;

    [Header("Coin Spawning")]
    public GameObject coinPrefab;
    public Vector2 coinSpawnInterval = new Vector2(1.5f, 3.5f);
    public Vector2 coinHeightRange = new Vector2(0.5f, 2.5f);
    private Coroutine coinCoroutine;

    [Header("General Power-Up Settings")]
    public Vector2 powerUpHeightRange = new Vector2(0.5f, 2.5f);

    [Header("Shield Power-Ups")]
    public GameObject shieldPrefab;
    public Vector2 shieldSpawnInterval = new Vector2(15f, 30f);
    private Coroutine shieldCoroutine;

    [Header("Coin Multiplier Power-Up")]
    public GameObject multiplierPrefab;
    public GameObject multiplierIcon;
    public Vector2 multiplierSpawnInterval = new Vector2(25f, 50f);
    public float multiplierDuration = 10.0f;
    public int multiplierValue = 2;

    public int CurrentCoinMultiplier { get; private set; } = 1;

    [Header("Time Slow Power-Ups")]
    public GameObject timeSlowPrefab;
    public Vector2 timeSlowSpawnInterval = new Vector2(25f, 45f);
    public float timeSlowMultiplier = 0.5f;
    public float timeSlowDuration = 5.0f;

    private Coroutine timeSlowSpawnCoroutine;
    private Coroutine activeTimeSlowCoroutine;
    private bool isTimeSlowed = false;

    private Coroutine multiplierSpawnCoroutine;
    private Coroutine activeMultiplierCoroutine;

    [Header("Boost Power-Up")]
    public GameObject boostPrefab;
    public Vector2 boostSpawnInterval = new Vector2(30f, 60f);
    public float boostDuration = 4.0f;
    public float boostTimeScale = 2.5f;

    public bool IsBoostActive { get; private set; } = false;

    private Coroutine boostSpawnCoroutine;
    private Coroutine activeBoostCoroutine;

    [Header("Power-Up Upgrades")]
    public UpgradeData boostDurationUpgrade;
    public UpgradeData multiplierDurationUpgrade;
    public UpgradeData timeSlowDurationUpgrade;

    [Header("Shop UI")]
    public GameObject shopBtn;
    public GameObject shopPanel;

    private int score = 0;
    public bool IsGamePlaying { get; private set; } = false;
    public bool IsGamePaused { get; private set; } = false;

    public TextMeshProUGUI scoreText;

    public GameObject playBtn;
    public GameObject pauseBtn;
    public GameObject pausePanel;
    public GameObject buttonPanel;

    public GameObject player;
    public DayNightCycle dayNightCycle;

    private Coroutine spawnCoroutine;
    private Coroutine gameOverCoroutine;
    private PlayerController playerController;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    public AudioSource musicSource;
    private bool hasGameEnded = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerController = player.GetComponent<PlayerController>();
        player.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        pauseBtn.SetActive(false);
        shopPanel.SetActive(false);
        shopBtn.SetActive(true);
    }

    private IEnumerator SpawnObstacle()
    {
        while (IsGamePlaying)
        {
            float waitTime = 1.2f;
            float currentSpeed = 10f;

            if (difficultyData != null)
            {
                Vector2 intervalRange = difficultyData.GetSpawnIntervalRange(score);
                waitTime = Random.Range(intervalRange.x, intervalRange.y);
                currentSpeed = difficultyData.GetObstacleSpeed(score);
            }
            else
            {
                waitTime = Random.Range(0.8f, 2f);
            }
            yield return new WaitForSeconds(waitTime);

            ObstacleData chosenData = GetRandomObstacle();
            Vector3 finalSpawnPos = spawnPoint.position + chosenData.spawnOffset;

            GameObject spawnedObj = PooledObject.Instance.Get(
    chosenData.prefab,
    finalSpawnPos,
    chosenData.prefab.transform.rotation
);

            Obstacles obsScript = spawnedObj.GetComponent<Obstacles>();
            obsScript.Initialize(chosenData, currentSpeed);
        }
    }

    private IEnumerator SpawnCoins()
    {
        while (IsGamePlaying)
        {
            float waitTime = Random.Range(coinSpawnInterval.x, coinSpawnInterval.y);
            yield return new WaitForSeconds(waitTime);

            if (coinPrefab == null) continue;

            // Current speed based on difficulty curve
            float currentSpeed = (difficultyData != null)
                ? difficultyData.GetObstacleSpeed(score)
                : 10f;

            // Pick a random Y between ground level and max jump height
            float randomY = Random.Range(coinHeightRange.x, coinHeightRange.y);
            Vector3 coinPos = new Vector3(
                spawnPoint.position.x,
                spawnPoint.position.y + randomY,
                spawnPoint.position.z
            );

            GameObject spawnedCoin = PooledObject.Instance.Get(
                coinPrefab,
                coinPos,
                Quaternion.identity
            );

            Coin coinScript = spawnedCoin.GetComponent<Coin>();
            if (coinScript != null)
            {
                coinScript.speed = currentSpeed;
            }
        }
    }

    private IEnumerator SpawnShields()
    {
        while (IsGamePlaying)
        {
            float waitTime = Random.Range(shieldSpawnInterval.x, shieldSpawnInterval.y);
            yield return new WaitForSeconds(waitTime);

            if (shieldPrefab == null) continue;

            float currentSpeed = (difficultyData != null)
                ? difficultyData.GetObstacleSpeed(score)
                : 10f;

            float randomY = Random.Range(powerUpHeightRange.x, powerUpHeightRange.y);
            Vector3 spawnPos = new Vector3(
                spawnPoint.position.x,
                spawnPoint.position.y + randomY,
                spawnPoint.position.z
            );

            GameObject spawnedShield = PooledObject.Instance.Get(
                shieldPrefab,
                spawnPos,
                Quaternion.identity
            );

            ShieldPickup shieldScript = spawnedShield.GetComponent<ShieldPickup>();
            if (shieldScript != null)
            {
                shieldScript.speed = currentSpeed;
            }
        }
    }

    public void ActivateTimeSlow()
    {
        if (isTimeSlowed && activeTimeSlowCoroutine != null)
        {
            StopCoroutine(activeTimeSlowCoroutine);
        }
        activeTimeSlowCoroutine = StartCoroutine(TimeSlowRoutine());
    }

    private IEnumerator TimeSlowRoutine()
    {
        isTimeSlowed = true;

        Time.timeScale = timeSlowMultiplier;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (musicSource != null) musicSource.pitch = timeSlowMultiplier;

        float finalTimeSlowDuration = timeSlowDuration;
        if (timeSlowDurationUpgrade != null) finalTimeSlowDuration *= timeSlowDurationUpgrade.GetCurrentValue();

        yield return new WaitForSecondsRealtime(finalTimeSlowDuration);

        isTimeSlowed = false;
        if (IsGamePlaying && !IsGamePaused)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            if (musicSource != null) musicSource.pitch = 1f;
        }
    }

    public void ActivateMultiplier()
    {
        if (activeMultiplierCoroutine != null) StopCoroutine(activeMultiplierCoroutine);
        activeMultiplierCoroutine = StartCoroutine(MultiplierRoutine());
    }

    private IEnumerator MultiplierRoutine()
    {
        CurrentCoinMultiplier = multiplierValue;
        if (multiplierIcon != null) multiplierIcon.SetActive(true);

        float finalMultiplierDuration = multiplierDuration;
        if (multiplierDurationUpgrade != null)
            finalMultiplierDuration *= multiplierDurationUpgrade.GetCurrentValue();

        yield return new WaitForSeconds(finalMultiplierDuration);

        CurrentCoinMultiplier = 1;
        if (multiplierIcon != null) multiplierIcon.SetActive(false);
    }

    private IEnumerator SpawnMultipliers()
    {
        while (IsGamePlaying)
        {
            float waitTime = Random.Range(multiplierSpawnInterval.x, multiplierSpawnInterval.y);
            yield return new WaitForSeconds(waitTime);

            if (multiplierPrefab == null) continue;

            float currentSpeed = (difficultyData != null) ? difficultyData.GetObstacleSpeed(score) : 10f;
            float randomY = Random.Range(powerUpHeightRange.x, powerUpHeightRange.y);
            Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + randomY, spawnPoint.position.z);

            GameObject spawnedObj = PooledObject.Instance.Get(multiplierPrefab, spawnPos, Quaternion.identity);

            MultiplierPickup pickupScript = spawnedObj.GetComponent<MultiplierPickup>();
            if (pickupScript != null) pickupScript.speed = currentSpeed;
        }
    }

    public void ActivateBoost()
    {
        if (activeBoostCoroutine != null) StopCoroutine(activeBoostCoroutine);
        activeBoostCoroutine = StartCoroutine(BoostRoutine());
    }

    private IEnumerator BoostRoutine()
    {
        IsBoostActive = true;
        Time.timeScale = boostTimeScale;

        float finalBoostDuration = boostDuration;
        if (boostDurationUpgrade != null)
            finalBoostDuration *= boostDurationUpgrade.GetCurrentValue();

        yield return new WaitForSecondsRealtime(finalBoostDuration);

        IsBoostActive = false;

        if (IsGamePlaying && !IsGamePaused)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                pc.BoostInvincibility();
            }
        }
    }

    private IEnumerator SpawnBoosts()
    {
        while (IsGamePlaying)
        {
            float waitTime = Random.Range(boostSpawnInterval.x, boostSpawnInterval.y);
            yield return new WaitForSeconds(waitTime);

            if (boostPrefab == null) continue;

            float currentSpeed = (difficultyData != null) ? difficultyData.GetObstacleSpeed(score) : 10f;
            float randomY = Random.Range(powerUpHeightRange.x, powerUpHeightRange.y);
            Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + randomY, spawnPoint.position.z);

            GameObject spawnedObj = PooledObject.Instance.Get(boostPrefab, spawnPos, Quaternion.identity);

            BoostPickup pickupScript = spawnedObj.GetComponent<BoostPickup>();
            if (pickupScript != null) pickupScript.speed = currentSpeed;
        }
    }

    private ObstacleData GetRandomObstacle()
    {
        float totalWeight = 0f;
        foreach (var obs in obstacleTypes)
        {
            totalWeight += obs.spawnWeight;
        }
        float randomValue = Random.Range(0, totalWeight);
        float currentWeightSum = 0f;

        foreach (var obs in obstacleTypes)
        {
            currentWeightSum += obs.spawnWeight;
            if (randomValue <= currentWeightSum)
            {
                return obs;
            }
        }
        return obstacleTypes[0];
    }

    private IEnumerator SpawnTimeSlows()
    {
        while (IsGamePlaying)
        {
            float waitTime = Random.Range(timeSlowSpawnInterval.x, timeSlowSpawnInterval.y);
            yield return new WaitForSeconds(waitTime);

            if (timeSlowPrefab == null) continue;

            float currentSpeed = (difficultyData != null) ? difficultyData.GetObstacleSpeed(score) : 10f;
            float randomY = Random.Range(powerUpHeightRange.x, powerUpHeightRange.y);

            Vector3 spawnPos = new Vector3(
                spawnPoint.position.x,
                spawnPoint.position.y + randomY,
                spawnPoint.position.z
            );

            GameObject spawnedTimeSlow = PooledObject.Instance.Get(timeSlowPrefab, spawnPos, Quaternion.identity);

            TimeSlowPickup tsScript = spawnedTimeSlow.GetComponent<TimeSlowPickup>();
            if (tsScript != null)
            {
                tsScript.speed = currentSpeed;
            }
        }
    }

    private IEnumerator GameOverCoroutine(float delay, int highScore)
    {
        yield return new WaitForSeconds(delay);

        finalScoreText.text = $"Score: {score}";
        highScoreText.text = $"Highest Score: {highScore}";
        gameOverPanel.SetActive(true);
        buttonPanel.SetActive(false);
        shopBtn.SetActive(true);
        scoreText.text = "";
    }

    private void ScoreUp()
    {
        if (!IsGamePlaying || IsGamePaused) return;
        score++;
        if (scoreText != null) scoreText.text = score.ToString();
        CheckBiomeProgression();
    }

    private void CheckBiomeProgression()
    {
        if (availableBiomes == null || availableBiomes.Length == 0) return;

        // Check if the next biome milestone has been reached
        int nextIndex = currentBiomeIndex + 1;
        if (nextIndex < availableBiomes.Length && score >= availableBiomes[nextIndex].scoreThreshold)
            TransitionToBiome(nextIndex);
    }

    private void TransitionToBiome(int biomeIndex, bool instant = false)
    {
        currentBiomeIndex = biomeIndex;
        BiomeData activateBiome = availableBiomes[currentBiomeIndex];

        if (dayNightCycle != null)
        {
            dayNightCycle.ApplyBiomeColors(activateBiome, instant);
        }

        if (groundRenderer != null && activateBiome.ground != null)
        {
            if (groundTransitionCoroutine != null) StopCoroutine(groundTransitionCoroutine);

            if (instant)
            {
                groundRenderer.material.color = activateBiome.ground.color;
            }
            else
            {
                float biomeTransitionDuration = 2.0f;
                groundTransitionCoroutine = StartCoroutine(TransitionGroundColor(activateBiome.ground.color, biomeTransitionDuration));
            }
        }

        if (activateBiome.biomeObstacles != null && activateBiome.biomeObstacles.Length > 0)
        {
            obstacleTypes = activateBiome.biomeObstacles;
        }
    }

    private IEnumerator TransitionGroundColor(Color targetColor, float duration)
    {
        if (groundRenderer == null) yield break;

        Material groundMat = groundRenderer.material;

        Color startColor = groundMat.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            groundMat.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        groundMat.color = targetColor;
    }

    public void GameStart()
    {
        isTimeSlowed = false;
        Time.fixedDeltaTime = 0.02f;
        if (musicSource != null) musicSource.pitch = 1f;

        IsGamePaused = false;
        IsGamePlaying = true;

        currentBiomeIndex = 0;
        if (availableBiomes != null && availableBiomes.Length > 0)
        {
            TransitionToBiome(0, true);
        }

        player.SetActive(true);
        playBtn.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        pauseBtn.SetActive(true);
        shopPanel.SetActive(false);
        shopBtn.SetActive(false);
        buttonPanel.SetActive(true);

        score = 0;
        scoreText.text = score.ToString();

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.time = 0f;
            musicSource.Play();
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.ResetCycle();
            dayNightCycle.StartCycle();
        }

        CurrentCoinMultiplier = 1;
        if (multiplierIcon != null) multiplierIcon.SetActive(false);

        if (multiplierSpawnCoroutine != null) StopCoroutine(multiplierSpawnCoroutine);
        multiplierSpawnCoroutine = StartCoroutine(SpawnMultipliers());

        CoinCollector collector = player.GetComponentInChildren<CoinCollector>();
        if (collector != null)
        {
            collector.UpdateCollectorRadius();
        }

        IsBoostActive = false;
        if (boostSpawnCoroutine != null) StopCoroutine(boostSpawnCoroutine);
        boostSpawnCoroutine = StartCoroutine(SpawnBoosts());

        if (shieldCoroutine != null)
            StopCoroutine(shieldCoroutine); shieldCoroutine = StartCoroutine(SpawnShields());

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine("SpawnObstacle");

        if (coinCoroutine != null) StopCoroutine(coinCoroutine);
        coinCoroutine = StartCoroutine(SpawnCoins());

        if (timeSlowSpawnCoroutine != null) StopCoroutine(timeSlowSpawnCoroutine);
        timeSlowSpawnCoroutine = StartCoroutine(SpawnTimeSlows());

        CancelInvoke(nameof(ScoreUp));
        InvokeRepeating("ScoreUp", 2f, 1f);
    }

    public void PauseGame()
    {
        if (!IsGamePlaying || IsGamePaused) return;

        IsGamePaused = true;
        Time.timeScale = 0f;

        musicSource.Pause();

        pausePanel.SetActive(true);
        pauseBtn.SetActive(false);
    }

    public void ResumeGame()
    {
        if (!IsGamePlaying || !IsGamePaused) return;

        IsGamePaused = false;
        Time.timeScale = isTimeSlowed ? timeSlowMultiplier : 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        musicSource.UnPause();

        pausePanel.SetActive(false);
        pauseBtn.SetActive(true);
    }

    public void GameOver(float delaySeconds)
    {
        if (!IsGamePlaying) return;

        hasGameEnded = true;
        IsGamePlaying = false;
        IsGamePaused = false;

        pauseBtn.SetActive(false);
        pausePanel.SetActive(false);
        multiplierIcon.SetActive(false);

        dayNightCycle.StopCycle();
        StopCoroutine(spawnCoroutine);
        CancelInvoke("ScoreUp");
        musicSource.Stop();

        Obstacles[] allObstacles = FindObjectsByType<Obstacles>();
        foreach (Obstacles obs in allObstacles)
        {
            obs.speed = 0f;
        }

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);

        if (gameOverCoroutine != null) StopCoroutine(gameOverCoroutine);
        gameOverCoroutine = StartCoroutine(GameOverCoroutine(delaySeconds, highScore));

        if (coinCoroutine != null) StopCoroutine(coinCoroutine);

        if (timeSlowSpawnCoroutine != null) StopCoroutine(timeSlowSpawnCoroutine);
        if (activeTimeSlowCoroutine != null) StopCoroutine(activeTimeSlowCoroutine);

        if (multiplierSpawnCoroutine != null) StopCoroutine(multiplierSpawnCoroutine);
        if (activeMultiplierCoroutine != null) StopCoroutine(activeMultiplierCoroutine);

        if (boostSpawnCoroutine != null) StopCoroutine(boostSpawnCoroutine);
        if (activeBoostCoroutine != null) StopCoroutine(activeBoostCoroutine);
    }

    public void RestartGame()
    {
        hasGameEnded = false;
        Time.timeScale = 1f;
        IsGamePaused = false;

        if (PooledObject.Instance != null)
        {
            PooledObject.Instance.DeactivateAll();
        }

        currentBiomeIndex = 0;
        if (availableBiomes != null && availableBiomes.Length > 0)
        {
            TransitionToBiome(0, true);
        }

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        if (gameOverCoroutine != null) StopCoroutine(gameOverCoroutine);
        if (coinCoroutine != null) StopCoroutine(coinCoroutine);

        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);

        if (timeSlowSpawnCoroutine != null) StopCoroutine(timeSlowSpawnCoroutine);
        if (activeTimeSlowCoroutine != null) StopCoroutine(activeTimeSlowCoroutine);

        if (multiplierSpawnCoroutine != null) StopCoroutine(multiplierSpawnCoroutine);
        if (activeMultiplierCoroutine != null) StopCoroutine(activeMultiplierCoroutine);

        if (boostSpawnCoroutine != null) StopCoroutine(boostSpawnCoroutine);
        if (activeBoostCoroutine != null) StopCoroutine(activeBoostCoroutine);

        if (playerController != null)
        {
            playerController.ResetPlayer();
        }

        multiplierIcon.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        buttonPanel.SetActive(true);

        GameStart();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        playBtn.SetActive(false);
        shopBtn.SetActive(false);

        if (gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);

        if (!IsGamePlaying)
        {
            if (hasGameEnded)
            {
                gameOverPanel.SetActive(true);
                shopBtn.SetActive(true);
                playBtn.SetActive(false);
            }
            else
            {
                playBtn.SetActive(true);
                shopBtn.SetActive(true);
                gameOverPanel.SetActive(false);
            }
        }
    }
}
