using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpDefaultGun : Interactables
{
    private string promptText;

    // picking it up
    public override void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Player Interacting");

        player.EquipDefaultGun();
        RemovePrompt();
        DestroyIntractable();
        Destroy(gameObject, .5f);
        Tutorial.Instance.ProgressTutorial();
    }
    // prompting player
    public override void ItemPrompt()
    {
        Debug.Log("Prompting Player");
        promptText = $"Press {player.pInput.GetInteractionKey()} To Pick Up Your Gun";
        player.pUI.EnablePrompt(promptText);
    }

    public override void RemovePrompt()
    {
        if (player == null)
            return;
        player.pUI.DisablePrompt();
    }
}
