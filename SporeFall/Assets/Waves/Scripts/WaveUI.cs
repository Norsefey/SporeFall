using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{

    //This script is supposed to make the bars at the top fill out as you kill enemies, showing your progression through the wave

    [SerializeField] private WaveManager waveManager;
    
    //Split up into separate bars because each wave has a different number of enemies
    [SerializeField] private Slider wave1Bar;
    [SerializeField] private Slider wave2Bar;
    [SerializeField] private Slider wave3Bar;
    //I'll deal with the final wave once the others are sorted out
    //[SerializeField] private Slider finalWaveBar;

    //Used for testing purposes
    //public int deadEnemies = 0;



    void Start()
    {
        //Attempting to grab the total enemies so the bars will count up as enemies are killed
        wave1Bar.maxValue = waveManager.waves[0].totalEnemies;
        wave2Bar.maxValue = waveManager.waves[1].totalEnemies;
        wave3Bar.maxValue = waveManager.waves[2].totalEnemies;
        //finalWaveBar.maxValue = waveManager.waves[3].totalEnemies;
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


    }
}
