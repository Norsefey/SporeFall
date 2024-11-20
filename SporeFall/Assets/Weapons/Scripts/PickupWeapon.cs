using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpWeapon : Interactables
{
    private string promptText;
    [Space(5), Header("Pick Up")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private float rotSpeed = 45;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound;      // Audio clip for pickup sound
    [Range(0f, 1f)] [SerializeField] private float pickupVolume = 0.5f; // Volume of pickup sound

    private AudioSource audioSource;
    private float timer = 10;

    private void Start()
    {
        // Add an AudioSource component to play the pickup sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = pickupSound;
        audioSource.volume = pickupVolume;

        timer = 9;
    }
    private void Update()
    {
        timer -= Time.deltaTime;
        // auto Despawn
        if (timer <= 0)
        {
            if (player != null)
                RemoveAction();
            DestroyIntractable();
        }
    }
    private void LateUpdate()
    {
        weapon.transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
    }

    public override void ItemAction()
    {
        player.nearByWeapon = weapon.gameObject;
        promptText = $"Press {player.pInput.GetInteractionKey()} To Pick Up: {weapon.weaponName}";
        player.pUI.EnablePrompt(promptText);
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        player.PickUpWeapon();
        RemoveAction();
        // Play the pickup sound effect
        if (pickupSound != null && audioSource != null)
        {
            audioSource.Play();
        }
        // Hide the weapon's visuals immediately
        weapon.gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);

        // Delay the destruction of the GameObject to allow the sound to finish
        Invoke(nameof(DestroyIntractable), pickupSound.length);
        Debug.Log("Picked up: " + weapon.name);
    }


    public override void RemoveAction()
    {
        timer = 5;
        player.nearByWeapon = null;
        player.pUI.DisablePrompt();
    }
}
