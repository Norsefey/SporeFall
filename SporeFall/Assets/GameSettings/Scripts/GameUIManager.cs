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


    public void ToggleUpgradeMenu(bool toggle)
    {
        if(trainUI != null)
            trainUI.SetActive(!toggle); 
        if(waveUI != null)
            waveUI.SetActive(!toggle);
        if (gameplayUI != null)
            gameplayUI.SetActive(!toggle);

        upgradeMenu.SetActive(toggle);
        UpgradeUI upUI = upgradeMenu.GetComponent<UpgradeUI>();
        if(upUI != null)
            upUI.SetSelectable();
    }
    public void DisplayMycelia(float value)
    {
        if (myceliaIndicator != null)
            myceliaIndicator.text = "Mycelia: " + value.ToString();
    }
}
