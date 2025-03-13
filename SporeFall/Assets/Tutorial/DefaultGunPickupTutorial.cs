using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultGunPickupTutorial : Interactables
{
    [SerializeField] DoorInteractable doorToOpen;
    private string promptText;

    // picking it up
    public override void Interact(InputAction.CallbackContext context)
    {
        if (player.currentWeapon != null)
            return;
        Debug.Log("Player Interacting");
        RemovePrompt();
        player.EquipDefaultGun();
        doorToOpen.UnlockDoor();
        Tutorial.Instance.ProgressTutorial();
        DestroyIntractable();
    }
    // prompting player
    public override void ItemPrompt()
    {
        Debug.Log("Prompting Player");
        if (player.currentWeapon != null)
            promptText = $"You already have a gun";
        else
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
