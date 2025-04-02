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
    [SerializeField] private TMP_Text myceliaIndicator;
    public void DisplayMycelia(float value)
    {
        if (myceliaIndicator != null)
            myceliaIndicator.text = "Mycelia: " + value.ToString();
    }

    public void ToggleGameUI(bool toggle)
    {
        if (trainUI != null)
            trainUI.SetActive(toggle);
        if (waveUI != null)
            waveUI.SetActive(toggle);
        if (gameplayUI != null)
            gameplayUI.SetActive(toggle);
    }
}
