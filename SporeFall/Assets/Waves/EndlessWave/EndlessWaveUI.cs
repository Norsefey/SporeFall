using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndlessWaveUI : MonoBehaviour
{
    [SerializeField] EndlessWaveManager waveManager;
    [SerializeField] HighScoreManager highScoreManager;

    [SerializeField] private GameObject startPrompt;

    [Header("During game UI")]
    [SerializeField] private GameObject survivalPrompt;
    [SerializeField] private TMP_Text survivalTimerText;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private TMP_Text EnemiesDefeatedText;

    [Header("ScoreBoard")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Transform scoreContainer;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private GameObject firstPlaceEntryPrefab;

    [Header("Downtime UI")]
    [SerializeField] private GameObject downtimePanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI waveCompleteText;
    [SerializeField] private TextMeshProUGUI nextWaveText;
    [SerializeField] private Slider countdownSlider;

    [Header("Downtime Settings")]
    private string waveCompleteMessage = "WAVE COMPLETE";
    private string nextWaveFormat = "NEXT WAVE: {0}";

    private float timer = 0;
    private int totalDeadEnemies = 0;
    private int totalDeadBosses = 0;
    private int currentWaveNumber;
    private bool isDowntimeActive = false;

    float bossPoints = 101;
    float mobPoints = 13;
    float timePoints = 2;
    float finalScore = 0;

    private float highestScore;
    private void Start()
    {
        survivalTimerText.text = "Survive \n" + string.Format("{0:0}:{1:00}", 0, 0);
        if (waveManager != null)
        {
            waveManager.inputManager.onPlayerJoined += DisableStartPrompt;

            waveManager.onWaveNumberChanged.AddListener(OnWaveNumberChanged);
            waveManager.onWaveDowntimeStarted.AddListener(OnWaveDowntimeStarted);
            waveManager.onWaveDowntimeProgress.AddListener(OnWaveDowntimeProgress);
            waveManager.onWaveDowntimeEnded.AddListener(OnWaveDowntimeEnded);
        }
    }
    private void Update()
    {
        if(waveManager.currentState != EndlessWaveManager.WaveState.NotStarted)
        {
            timer += Time.deltaTime;

            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            survivalTimerText.text = "Survive \n" + string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }
    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (waveManager != null)
        {
            waveManager.onWaveNumberChanged.RemoveListener(OnWaveNumberChanged);
            waveManager.onWaveDowntimeStarted.RemoveListener(OnWaveDowntimeStarted);
            waveManager.onWaveDowntimeProgress.RemoveListener(OnWaveDowntimeProgress);
            waveManager.onWaveDowntimeEnded.RemoveListener(OnWaveDowntimeEnded);
        }
    }
    public void ShowGameOverPanel()
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);
        finalTimeText.text = $"Survival Time:  {string.Format("{0:0}:{1:00}", minutes, seconds)}";
        EnemiesDefeatedText.text = $"Enemies Defeated: {totalDeadEnemies} \n Bosses Defeated: {totalDeadBosses}";
        CalculateFinalScore();
        gameOverPanel.SetActive(true);
    }
    public void IncreaseDeadEnemies()
    {
        totalDeadEnemies++;
    }
    public void IncreaseDeadBosses()
    {
        totalDeadBosses++;
    }
    private void DisableStartPrompt(PlayerInput playerInput)
    {
        startPrompt.SetActive(false);
        waveManager.inputManager.onPlayerJoined -= DisableStartPrompt;
    }
    private void CalculateFinalScore()
    {
        float finalBossScore = bossPoints * totalDeadBosses;
        float finalEnemyScore = mobPoints * totalDeadEnemies;
        float finalTimeScore = Mathf.FloorToInt(timePoints * timer);

        finalScore = finalBossScore + finalEnemyScore + finalTimeScore;

        // Add score to high scores list
        bool isHighScore = highScoreManager.AddScore("lv3", (int)finalScore);

        ShowHighScores("lv3");

        // You might also want to show a special message for new high scores
        if (isHighScore && finalScore > highestScore)
        {
            Debug.Log("New High Score!");
            // Show high score animation or special UI effect
            finalScoreText.text = $" <color=yellow>!!New High Score!! \n {finalScore.ToString("F0")}</color>";
        }else if (isHighScore)
        {
            finalScoreText.text = $" <color=yellow>!In Top 5! \n {finalScore.ToString("F0")}</color>";
        }
        else
        {
            finalScoreText.text = $"Final Score \n {finalScore.ToString("F0")}";
        }
    }
    public void ShowHighScores(string levelName)
    {
        // Clear previous entries
        foreach (Transform child in scoreContainer)
        {
            Destroy(child.gameObject);
        }

        // Get scores for the level
        List<int> scores = highScoreManager.GetHighScores(levelName);

        // Display scores
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject scoreEntryGO;
            if (i == 0)
            {
                highestScore = scores[i];
                scoreEntryGO = Instantiate(firstPlaceEntryPrefab, scoreContainer);
            }
            else
            {
                scoreEntryGO = Instantiate(scoreEntryPrefab, scoreContainer);
            }

            // Find child text elements
            TMP_Text rankText = scoreEntryGO.transform.Find("RankText")?.GetComponent<TMP_Text>();
            TMP_Text scoreText = scoreEntryGO.transform.Find("ScoreText")?.GetComponent<TMP_Text>();

            if (rankText != null)
                rankText.text = (i + 1).ToString();

            if (scoreText != null)
                scoreText.text = scores[i].ToString("N0");
        }
    }

    #region DownTime
    private void OnWaveNumberChanged(int waveNumber)
    {
        currentWaveNumber = waveNumber;

        // Update the next wave text
        if (nextWaveText != null)
        {
            nextWaveText.text = string.Format(nextWaveFormat, currentWaveNumber);
        }
    }
    private void OnWaveDowntimeStarted()
    {
        isDowntimeActive = true;
        survivalPrompt.SetActive(false);
        SetDowntimeUIVisible(true);

        // Display wave complete message
        if (waveCompleteText != null)
        {
            waveCompleteText.text = waveCompleteMessage;
        }
        // Set next wave text
        if (nextWaveText != null)
        {
            nextWaveText.text = string.Format(nextWaveFormat, currentWaveNumber);
        }
    }

    private void OnWaveDowntimeProgress(float progress)
    {
        if (!isDowntimeActive) return;

        // Convert progress (0-1) to remaining time
        float remainingTime = waveManager.DowntimeDuration * (1 - progress);

        // Update the countdown text
        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
        }

        // Update slider if available
        if (countdownSlider != null)
        {
            countdownSlider.value = progress;
        }
    }
    private void OnWaveDowntimeEnded()
    {
        isDowntimeActive = false;
        SetDowntimeUIVisible(false);
        survivalPrompt.SetActive(true);

    }

    private void SetDowntimeUIVisible(bool visible)
    {
        if (downtimePanel != null)
        {
            downtimePanel.SetActive(visible);
        }
    }

    public void SkipDowntime()
    {
        if (isDowntimeActive && waveManager != null)
        {
            waveManager.SkipDowntime();
        }
    }
    #endregion
}
