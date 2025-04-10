using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveButton : Interactables
{

    private int buttonPress = 0;

    public override void ItemPrompt()
    {
        switch (GameManager.Instance.waveManager.wavePhase)
        {
            case WaveManager.WavePhase.NotStarted:
                player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()}  to Start Wave");
                break;
           /* case WaveManager.WavePhase.Departing:
                player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} to go to next Area");
                break;*/
        }
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        switch (GameManager.Instance.waveManager.wavePhase)
        {
            case WaveManager.WavePhase.NotStarted:
                buttonPress++;
                
                if (Tutorial.Instance != null && buttonPress == 1)
                {
                    Tutorial.Instance.tutorialPrompt = 5;
                    Tutorial.Instance.ProgressTutorial();
                }

                GameManager.Instance.waveManager.OnStartWave();
                player.pUI.DisablePrompt();
                break;
        /*    case WaveManager.WavePhase.Departing:
                GameManager.Instance.waveManager.SkipDepartTime();
                player.pInput.RemoveInteraction(this);
                RemovePrompt();
                break;*/
            default:
                Debug.Log("No Action");
                RemoveIntractable();
                break;
        }
    }

    public override void RemovePrompt()
    {
        player.pUI.DisablePrompt();
    }
}
