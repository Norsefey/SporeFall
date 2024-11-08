using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeShop : Interactables
{
    public override void ItemAction()
    {
        player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} To Open Menu");
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        //Enable Upgrade Menu Ui
    }

    public override void RemoveAction()
    {
        player.pUI.DisablePrompt();
    }
}
