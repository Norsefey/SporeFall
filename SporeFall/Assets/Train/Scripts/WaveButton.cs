using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class WaveButton : Interactables
{
    public override void AssignAction()
    {
        switch (WaveManager.Instance.wavePhase)
        {
            case WaveManager.WavePhase.NotStarted:
                player.pUI.EnablePrompt("Press to Start Wave");
                break;
            case WaveManager.WavePhase.Departing:
                player.pUI.EnablePrompt("Press to go to next Area");
                break;
        }
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        switch (WaveManager.Instance.wavePhase)
        {
            case WaveManager.WavePhase.NotStarted:
                WaveManager.Instance.OnStartWave();
                player.pUI.DisablePrompt();
                break;
            case WaveManager.WavePhase.Departing:
                WaveManager.Instance.SkipDepartTime();
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
