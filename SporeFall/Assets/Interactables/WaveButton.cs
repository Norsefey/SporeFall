using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class WaveButton : Interactables
{

    private int buttonPress = 0;

    public override void ItemAction()
    {
        switch (GameManager.Instance.waveManager.wavePhase)
        {
            case WaveManager.WavePhase.NotStarted:
                player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()}  to Start Wave");
                break;
            case WaveManager.WavePhase.Departing:
                player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} to go to next Area");
                break;
        }
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        switch (GameManager.Instance.waveManager.wavePhase)
        {
            case WaveManager.WavePhase.NotStarted:
                buttonPress++;
                if (buttonPress == 1)
                {
                    Tutorial.Instance.tutorialPrompt = 5;
                    Tutorial.Instance.ProgressTutorial();
                }
                GameManager.Instance.waveManager.OnStartWave();
                player.pUI.DisablePrompt();
                break;
            case WaveManager.WavePhase.Departing:
                GameManager.Instance.waveManager.SkipDepartTime();
                player.pInput.RemoveInteraction(this);
                RemoveAction();
                break;
            default:
                Debug.Log("No Action");
                break;
        }
    }

    public override void RemoveAction()
    {
        player.pUI.DisablePrompt();
    }
}