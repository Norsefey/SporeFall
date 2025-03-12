using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CorruptionReduction : Interactables
{
    public override void ItemPrompt()
    {
        player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} To Purchase Corruption Reduction");
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        if (player.pCorruption.TryPurchaseCorruptionReduction())
        {
            Debug.Log("Purchased Reduction");
        }
        else
        {
            StartCoroutine(player.pUI.ShowInsufficientFundsWarning("Cannot Not Purchase"));
        }
    }

    public override void RemovePrompt()
    {
        player.pUI.DisablePrompt();
    }
}
