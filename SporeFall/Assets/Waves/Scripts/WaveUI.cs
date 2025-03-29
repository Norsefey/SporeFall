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
    [SerializeField] private Slider bossBar;
    [SerializeField] private Slider finalWaveBar;
    [Header("Flags")]
    [SerializeField] private Image wave1Flag;
    [SerializeField] private Image wave2Flag;
    [SerializeField] private Image wave3Flag;
    [SerializeField] private Image bossFlag;
    [SerializeField] private Image finalWaveFlag;
    [Header("Sprites")]
    [SerializeField] private Sprite podFlagSprite;
    [SerializeField] private Sprite bossFlagSprite;
    [Header("Text")]
    [SerializeField] private GameObject waveTextHolder;
    [SerializeField] private TMP_Text waveText;
    //[SerializeField] private GameObject waveCountdownTextHolder;
    //[SerializeField] private TMP_Text waveCountdownText;

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
        waveTextHolder.SetActive(false);
        //waveCountdownTextHolder.SetActive(false);
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
        ///y- I would recommend using if else here, same with the Displayflags
        /// if you just use ifs, it checks through all of them, whereas with if else, it stops at the first true statement
        /// and in here you can stop at the first true
        ///If you want to go a little more advance with it I would recommend using a switch statement, something like this:
        
      /*  switch (waveManager.currentWaveIndex)
        {
            case 0:
                wave1Bar.value = value;
                break;
            case 1:
                wave2Bar.value = value;
                break; // We break meaning leave the switch statement
            // then all the others
            
            // a default can be put in as a catch all in case something goes wrong
            default:
                break;
        }*/


        if (waveManager.currentWaveIndex == 0)
        {
            wave1Bar.value = value;
            //Debug.Log("Updating wave 1 bar");
        }

        else if(waveManager.currentWaveIndex == 1)
        {
            wave2Bar.value = value;
            //Debug.Log("Updating wave 2 bar");
        }

        else if(waveManager.currentWaveIndex == 2)
        {
            wave3Bar.value = value;
            //Debug.Log("Updating wave 3 bar");
        }

        else if(waveManager.currentWaveIndex == 3)
        {
            finalWaveBar.value = value;
            //Debug.Log("Updating final wave bar");
        }
    }

    public void DisplayBossProgress()
    {
        bossBar.value = bossBar.maxValue;
    }

    public void DisplayWaveFlags()
    {
        if (waveManager.currentWaveIndex == 0)
        {
            wave1Flag.sprite = podFlagSprite;
        }

        else if(waveManager.currentWaveIndex == 1)
        {
            wave2Flag.sprite = podFlagSprite;
        }

        else if(waveManager.currentWaveIndex == 2)
        {
            wave3Flag.sprite = podFlagSprite;
        }

        else if(waveManager.currentWaveIndex == 3)
        {
            finalWaveFlag.sprite = podFlagSprite;
        }
    }

    public void DisplayBossFlag()
    {
        bossFlag.sprite = bossFlagSprite;
    }

    public void DisplayWaveStart()
    {
        waveTextHolder.SetActive(true);
        ///y- I took a peek and realized i never made Current wave public, so I made it public
        /// Each wave has a give name that can be called by .waveName, you can see it in the wave manager
        /// They also have a public .IsFinalWave bool
        /// I commented it out, but this could make things easier for you, as you would not need to check the wave index or if it is final wave
        /// Neat thing i found recently if you put a $ in at the start before the "", you don't have to use + symbols, just put none text things in {}
        
        //waveText.text = $"{waveManager.CurrentWave.waveName} Start";

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
            if (Tutorial.Instance != null && waveManager.currentWaveIndex == 0)
            {
                Tutorial.Instance.StartBetweenWaveTutorial();
            }
        }
        StartCoroutine(WaveTextCooldown());
    }

    IEnumerator WaveTextCooldown()
    {
        yield return new WaitForSeconds(3);
        waveTextHolder.SetActive(false);
    }
}
