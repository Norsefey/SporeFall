using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeShop : Interactables
{
    // prompt player to interact with
    public override void ItemAction()
    {
        if (GameManager.Instance.WaveManager.wavePhase == WaveManager.WavePhase.NotStarted)
        {
            player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()}  to Open Shop");
        }
    
    }
    // what it does once player interacts
    public override void Interact(InputAction.CallbackContext context)
    {
        // Open Shop menu
        // ESC now closes Shop Menu instead of pause
        if (GameManager.Instance.WaveManager.wavePhase == WaveManager.WavePhase.NotStarted)
        {
            GameManager.Instance.GameUIManager.ShowUpgradeMenu(true);
            player.pInput.ToggleUpgradeMenu(true);
            player.pUI.DisablePrompt();

            // Update upgrade UI

        }
    }
    // remove prompt and anything else
    public override void RemoveAction()
    {
        player.pUI.DisablePrompt();
    }
}
