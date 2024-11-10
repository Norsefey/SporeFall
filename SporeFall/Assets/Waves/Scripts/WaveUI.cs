using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    public static WaveUI Instance;
    //This script is supposed to make the bars at the top fill out as you kill enemies, showing your progression through the wave

    [SerializeField] private WaveManager waveManager;

    [Header("Bars")]
    //Split up into separate bars because each wave has a different number of enemies
    [SerializeField] private Slider wave1Bar;
    [SerializeField] private Slider wave2Bar;
    [SerializeField] private Slider wave3Bar;
    [SerializeField] private Slider finalWaveBar;
    [Header("Flags")]
    [SerializeField] private GameObject wave1Flag;
    [SerializeField] private GameObject wave2Flag;
    [SerializeField] private GameObject wave3Flag;
    [SerializeField] private GameObject finalWaveFlag;
    [Header("Other")]
    [SerializeField] private GameObject waveTextHolder;
    [SerializeField] private TMP_Text waveText;

    //Used for testing purposes
    //public int deadEnemies = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //Attempting to grab the total enemies so the bars will count up as enemies are killed
        wave1Bar.maxValue = waveManager.waves[0].totalEnemies;
        wave2Bar.maxValue = waveManager.waves[1].totalEnemies;
        wave3Bar.maxValue = waveManager.waves[2].totalEnemies;
        finalWaveBar.maxValue = waveManager.payloadPath.Length;
        wave1Flag.SetActive(false);
        wave2Flag.SetActive(false);
        wave3Flag.SetActive(false);
        finalWaveFlag.SetActive(false);
        waveTextHolder.SetActive(false);
    }

    
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    deadEnemies++;
        //    DisplayWaveProgress(deadEnemies);
        //    Debug.Log("Enemy died");
        //}
    }

    public void DisplayWaveProgress(int value)
    {
        if (waveManager.currentWaveIndex == 0)
        {
            wave1Bar.value = value;
            Debug.Log("Updating wave 1 bar");
        }

        if (waveManager.currentWaveIndex == 1)
        {
            wave2Bar.value = value;
            Debug.Log("Updating wave 2 bar");
        }

        if (waveManager.currentWaveIndex == 2)
        {
            wave3Bar.value = value;
            Debug.Log("Updating wave 3 bar");
        }

        if (waveManager.currentWaveIndex == 3)
        {
            finalWaveBar.value = value;
            Debug.Log("Updating final wave bar");
        }


    }

    public void DisplayWaveFlags()
    {
        if (waveManager.currentWaveIndex == 0)
        {
            wave1Flag.SetActive(true);
        }

        if (waveManager.currentWaveIndex == 1)
        {
            wave2Flag.SetActive(true);
        }

        if (waveManager.currentWaveIndex == 2)
        {
            wave3Flag.SetActive(true);
        }

        if (waveManager.currentWaveIndex == 3)
        {
            finalWaveFlag.SetActive(true);
        }
    }

    public void DisplayWaveStart()
    {
        waveTextHolder.SetActive(true);
        if (waveManager.currentWaveIndex <= 2)
        {
            waveText.text = "Wave " + (waveManager.currentWaveIndex + 1) + " Start";
        }
        else
        {
            waveText.text = "Final Wave Start";
        }
        StartCoroutine(WaveTextCooldown());
    }

    public void DisplayWaveClear()
    {
        waveTextHolder.SetActive(true);
        if (waveManager.currentWaveIndex <= 2)
        {
            waveText.text = "Wave " + (waveManager.currentWaveIndex + 1) + " Clear";
        }
        StartCoroutine(WaveTextCooldown());
    }

    IEnumerator WaveTextCooldown()
    {
        yield return new WaitForSeconds(3);
        waveTextHolder.SetActive(false);
    }
}
