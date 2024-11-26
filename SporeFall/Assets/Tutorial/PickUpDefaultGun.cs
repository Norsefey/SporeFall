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
        RemoveAction();
        DestroyIntractable();
        Destroy(gameObject, .5f);
        Tutorial.Instance.ProgressTutorial();
    }
    // prompting player
    public override void ItemAction()
    {
        Debug.Log("Prompting Player");
        promptText = $"Press {player.pInput.GetInteractionKey()} To Pick Up Your Gun";
        player.pUI.EnablePrompt(promptText);
    }

    public override void RemoveAction()
    {
        if (player == null)
            return;
        player.pUI.DisablePrompt();
    }
}
