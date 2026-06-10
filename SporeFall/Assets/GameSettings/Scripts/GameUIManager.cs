// Ignore Spelling: gameplay

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject trainUI;
    [SerializeField] private GameObject waveUI;
    public GameObject gameplayUI;
    [SerializeField] private GameObject upgradeMenu;
    [SerializeField] private TMP_Text myceliaIndicator;
    [SerializeField] private TMP_Text energyText;
    public TMP_Text departText;

    public EndlessWaveUI endlessUI;

    private void Update()
    {
     if(Input.GetKeyDown(KeyCode.P))
        {
            GameManager.Instance.players[0].pUI.gameObject.SetActive(false);
            GameManager.Instance.gameUI.ToggleTutorialPrompts(true);
            if (GameManager.Instance.trainHandler != null)
                GameManager.Instance.trainHandler.UI.gameObject.SetActive(false);

            if (GameManager.Instance.waveManager != null && SavedSettings.currentLevel != "Training")
            {
                GameManager.Instance.waveManager.wUI.gameObject.SetActive(false);
            }
            gameplayUI.SetActive(false);
        }
    }

    public void DisplayMycelia(float value)
    {
        if (myceliaIndicator != null)
            myceliaIndicator.text = value.ToString("F1");
    }
    public void DisplayEnergy(float value)
    {
        if (energyText != null)
            energyText.text = value.ToString("F1");
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
    public void ToggleUpgradeMenu(bool toggle, PlayerManager player)
    {
        ToggleGameUI(!toggle);

        upgradeMenu.SetActive(toggle);

        UpgradeUI upUI = upgradeMenu.GetComponent<UpgradeUI>();
        if (upUI)
        {
            upUI.activePlayer = player;
            if (upUI != null)
                upUI.SetSelectable();
        }
    }
    public void ToggleTutorialPrompts(bool toggle)
    {
        if (Tutorial.Instance != null)
        {
            if (Tutorial.Instance.tutorialOngoing)
            {
                Tutorial.Instance.tutorialPopup.SetActive(!toggle);
            }
        }
    }
    public void EnableEndlessGameOver()
    {
        ToggleGameUI(false);
        upgradeMenu.SetActive(false);
        endlessUI.ShowGameOverPanel();
    }
}
