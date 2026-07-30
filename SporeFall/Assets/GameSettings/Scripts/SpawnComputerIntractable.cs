using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnComputerIntractable : Interactables
{
    [SerializeField] private SpawnMenu spawnMenu;

    public override void Interact(InputAction.CallbackContext context)
    {
        if (!interactionEnabled)
        {
            RemoveIntractable();
            return;
        }

        player.pUI.DisablePrompt();
        player.pInput.ToggleMenu(true);

        spawnMenu.OpenMenu();
    }

    public override void ItemPrompt()
    {
        player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} to Access");
    }

    public override void RemovePrompt()
    {
        if (player == null)
            return;
        player.pUI.DisablePrompt();
    }
}
