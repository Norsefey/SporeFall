using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndlessWaveUI : MonoBehaviour
{

    [SerializeField] EndlessWaveManager waveManager;

    [SerializeField]private TMP_Text timerText;
    public float timer = 0;

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
}
