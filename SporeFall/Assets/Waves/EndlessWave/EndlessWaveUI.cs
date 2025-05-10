using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndlessWaveUI : MonoBehaviour
{

    [SerializeField] EndlessWaveManager waveManager;
    
    [Header("During game UI")]
    [SerializeField] private TMP_Text timerText;


    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private TMP_Text EnemiesDefeatedText;
    private float timer = 0;
    private int totalDeadEnemies = 0;
    private int totalDeadBosses = 0;

    private void Start()
    {
        timerText.text = string.Format("{0:0}:{1:00}", 0, 0);
    }
    private void Update()
    {
        if(waveManager.currentState != EndlessWaveManager.WaveState.NotStarted)
        {
            timer += Time.deltaTime;

            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }
    public void ShowGameOverPanel()
    {
        finalTimeText.text = $"Survival Time:  {timerText.text}";
        EnemiesDefeatedText.text = $"Enemies Defeated: {totalDeadEnemies} \n Bosses Defeated: {totalDeadBosses}";
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
  
}
