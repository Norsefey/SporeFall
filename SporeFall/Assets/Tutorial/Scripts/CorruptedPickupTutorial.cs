using UnityEngine;
using UnityEngine.InputSystem;

public class CorruptedPickupTutorial : Interactables
{
    private string promptText;
    [Space(5), Header("Pick Up")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private ShootingRoomTutorial roomTutorial;
    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound;
    private AudioSource audioSource;
    public override void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Player Interacting");
        player.PickUpWeapon();
        RemovePrompt();

        if (pickupSound != null && audioSource != null)
        {
            audioSource.Play();
        }
        // Hide the weapon's visuals immediately
        weapon.gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);

        // Delay the destruction of the GameObject to allow the sound to finish playing
        Invoke(nameof(DestroyIntractable), pickupSound.length);
        Debug.Log("Picked up: " + weapon.name);

        roomTutorial.StartCorruptedWeaponTutorial();

        DestroyIntractable();
    }

    public override void ItemPrompt()
    {
        player.nearByWeapon = weapon.gameObject;
        promptText = $"Press {player.pInput.GetInteractionKey()} To Pick Up: {weapon.weaponName}";
        player.pUI.EnablePrompt(promptText);
    }

    public override void RemovePrompt()
    {
        if (player == null)
            return;
        player.nearByWeapon = null;
        player.pUI.DisablePrompt();
    }
}
