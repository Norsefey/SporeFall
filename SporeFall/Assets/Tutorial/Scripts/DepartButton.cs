using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DepartButton : Interactables
{
    public override void Interact(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }

    public override void ItemPrompt()
    {
        player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} to Depart");
    }

    public override void RemovePrompt()
    {
        if (player == null)
            return;
        player.pUI.DisablePrompt();
    }
}
