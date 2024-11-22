// Ignore Spelling: gameplay

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public GameObject trainUI;
    public GameObject waveUI;
    public GameObject gameplayUI;
    public GameObject upgradeMenu;
    public void ShowUpgradeMenu(bool toggle)
    {
        trainUI.SetActive(!toggle); 
        waveUI.SetActive(!toggle);
        gameplayUI.SetActive(!toggle);

        upgradeMenu.SetActive(toggle);
    }
}
