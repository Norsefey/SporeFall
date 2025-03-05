using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PickUpWeapon : Interactables
{
    private string promptText;
    [Space(5), Header("Pick Up")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private float rotSpeed = 45;

    [Header("Despawn Settings")]
    [SerializeField] private float despawnTime = 30f;    // Time in seconds before weapon despawns
    [SerializeField] private float blinkStartTime = 5f;  // Time before despawn when weapon starts blinking
    [SerializeField] private float blinkRate = 0.5f;     // How fast the weapon blinks (in seconds)

    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound;
    [Range(0f, 1f)][SerializeField] private float pickupVolume = 0.5f;

    private int pickupCount = 0;
    private AudioSource audioSource;
    private float despawnTimer;
    private bool isBlinking;
    private Coroutine blinkCoroutine;
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = pickupSound;
        audioSource.volume = pickupVolume;

        // Start despawn timer
        despawnTimer = despawnTime;

        // Start the despawn countdown
        StartCoroutine(DespawnCountdown());
    }
    private void LateUpdate()
    {
        weapon.transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
    }

    private IEnumerator DespawnCountdown()
    {
        while (despawnTimer > 0)
        {
            despawnTimer -= Time.deltaTime;

            // Start blinking when approaching despawn time
            if (despawnTimer <= blinkStartTime && !isBlinking)
            {
                isBlinking = true;
                blinkCoroutine = StartCoroutine(BlinkWeapon());
            }

            if (despawnTimer <= 0)
            {
                DespawnWeapon();
                yield break;
            }

            yield return null;
        }
    }
    private IEnumerator BlinkWeapon()
    {
        bool visible = true;

        while (isBlinking)
        {
            // Toggle visibility of weapon
            weapon.gameObject.SetActive(visible);

            visible = !visible;
            yield return new WaitForSeconds(blinkRate);
        }
    }

    private void DespawnWeapon()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        RemoveAction();
        DestroyIntractable();
    }

    public override void ItemAction()
    {
        player.nearByWeapon = weapon.gameObject;
        promptText = $"Press {player.pInput.GetInteractionKey()} To Pick Up: {weapon.weaponName}";
        player.pUI.EnablePrompt(promptText);
    }
    public override void Interact(InputAction.CallbackContext context)
    {
        // Stop all coroutines when picked up
        StopAllCoroutines();

        player.PickUpWeapon();
        RemoveAction();

        if (pickupCount < 1)
        {
            pickupCount++;
            if (Tutorial.Instance.currentScene == "Tutorial")
            {
                Tutorial.Instance.DestroyDoor(2);
            }
        }

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
        if(player == null)
            return;
        player.nearByWeapon = null;
        player.pUI.DisablePrompt();
    }

    private void OnDisable()
    {
        // Ensure renderers are enabled when object is returned to pool
        weapon.gameObject.SetActive(true);
        // Reset the despawn timer
        despawnTimer = despawnTime;
        isBlinking = false;
    }
}
