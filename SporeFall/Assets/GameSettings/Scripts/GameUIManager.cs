// Ignore Spelling: gameplay

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject trainUI;
    [SerializeField] private GameObject waveUI;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject upgradeMenu;
    [SerializeField] private TMP_Text myceliaIndicator;


    public void ShowUpgradeMenu(bool toggle)
    {
        trainUI.SetActive(!toggle); 
        waveUI.SetActive(!toggle);
        gameplayUI.SetActive(!toggle);

        upgradeMenu.SetActive(toggle);
    }
    public void DisplayMycelia(float value)
    {
        if (myceliaIndicator != null)
            myceliaIndicator.text = "Mycelia: " + value.ToString();
    }
}
