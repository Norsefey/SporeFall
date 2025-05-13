using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerRevive : Interactables
{
    [Header("Revive Settings")]
    [SerializeField] private float reviveTime = 3.0f;
    [SerializeField] private GameObject UIHolder;
    [SerializeField] private Slider reviveSliderUI;
    [SerializeField] private Slider deathSliderUI;


    private PlayerManager downedPlayer;
    private Coroutine reviveCoroutine;
    private bool isReviving = false;

    [Header("Audio")]
    [SerializeField] private AudioClip reviveStartSound;
    [SerializeField] private AudioClip reviveCompleteSound;
    [SerializeField] private AudioSource reviveAudioSource;

    private void Awake()
    {
        if(downedPlayer == null)
            downedPlayer = GetComponentInParent<PlayerManager>();
    }

    private void OnEnable()
    {
        if (reviveSliderUI != null)
        {
            reviveSliderUI.maxValue = 1;
            reviveSliderUI.value = 0;
        }

        if(deathSliderUI != null)
        {
            deathSliderUI.maxValue = 1;
            deathSliderUI.value = 0;
            StartCoroutine(DeathSliderUpdater());
        }

        // Set up audio source if not already assigned
        if (reviveAudioSource == null)
            reviveAudioSource = gameObject.AddComponent<AudioSource>();
    }
    public override void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Start the revive process when the button is initially pressed
            Debug.Log("Starting Revive");
            StartRevive();
        }
        else if (context.canceled)
        {
            // Cancel revive when the button is released before completion
            Debug.Log("Stopping Revive");
            CancelRevive();
        }
    }
    public override void ItemPrompt()
    {
        if(player == downedPlayer)
        {
            player = null;
            return;
        }

        // Show the revive prompt when a player approaches
        if (reviveSliderUI != null)
        {
            reviveSliderUI.value = 0;
            reviveSliderUI.enabled = true;
        }

        string promptText = "";
        int playerIndex = downedPlayer.GetPlayerIndex();
 
        promptText = $"Hold <color=yellow>{player.pInput.GetInteractionKey()}</color> to revive Aegis Unit {playerIndex}";
        
        player.pUI.EnablePrompt(promptText);
    }
    public override void RemovePrompt()
    {
        // Hide the revive prompt when a player leaves
        if (reviveSliderUI != null)
        {
            reviveSliderUI.value = 0;
            reviveSliderUI.enabled = false;
        }

        player.pUI.DisablePrompt();

        // Make sure to cancel any ongoing revive attempt
        CancelRevive();
    }
    private void StartRevive()
    {
        if (!isReviving && downedPlayer != null && downedPlayer.pHealth.isDieing)
        {
            Debug.Log("We can save them");

            isReviving = true;
            reviveCoroutine = StartCoroutine(ReviveProcess());

            // Play start revive sound
            if (reviveAudioSource != null && reviveStartSound != null)
                reviveAudioSource.PlayOneShot(reviveStartSound);
        }
    }
    private void CancelRevive()
    {
        if (isReviving)
        {
            isReviving = false;

            if (reviveCoroutine != null)
                StopCoroutine(reviveCoroutine);

            // Reset the progress bar
            reviveSliderUI.value = 0;
        }
    }
    private IEnumerator ReviveProcess()
    {
        float elapsedTime = 0f;
        Debug.Log("Reviving Please Wait");

        // Update the progress bar as the player holds the interact button
        while (elapsedTime < reviveTime)
        {
            elapsedTime += Time.deltaTime;
            // Update progress bar
            reviveSliderUI.value = (elapsedTime / reviveTime);

            downedPlayer.pHealth.deathTimeCounter -= 1.1f * Time.deltaTime;

            yield return null;
        }

        // Revive complete
        if (downedPlayer != null)
        {
            CompleteRevive();
        }

        isReviving = false;

        // Reset the progress bar
        reviveSliderUI.value = 0;

    }
    private void CompleteRevive()
    {
        // Stop the revive coroutine if it's running
        if (downedPlayer.respawnCoroutine != null)
            downedPlayer.StopCoroutine(downedPlayer.respawnCoroutine);

        // Play revive complete sound
        if (reviveAudioSource != null && reviveCompleteSound != null)
            reviveAudioSource.PlayOneShot(reviveCompleteSound);

        // Reset player state
        downedPlayer.TogglePControl(true);
        downedPlayer.TogglePCorruption(true);
        downedPlayer.pAnime.ToggleIKAim(true);
        downedPlayer.pAnime.ToggleRespawn(true);

        // Reset animation based on weapon
        if (downedPlayer.currentWeapon != null)
            downedPlayer.pAnime.SetWeaponHoldAnimation(downedPlayer.currentWeapon.holdType);
        else
            downedPlayer.pAnime.SetWeaponHoldAnimation(0);

        // Reset health
        downedPlayer.pHealth.Revive();
        downedPlayer.pHealth.RestoreHP(downedPlayer.pHealth.MaxHP / 2);

        downedPlayer.isRespawning = false;

        // Destroy the revive interaction
        gameObject.SetActive(false);
    }
    public IEnumerator DeathSliderUpdater()
    {
        if (downedPlayer == null)
            downedPlayer = GetComponentInParent<PlayerManager>();

        while (downedPlayer.pHealth.isDieing)
        {
            LookAtOtherPlayer();
            deathSliderUI.value = (downedPlayer.pHealth.deathTimeCounter / downedPlayer.pHealth.deathTime);
            yield return null;
        }

        UIHolder.transform.rotation = Quaternion.identity;
    }

    private void LookAtOtherPlayer()
    {
        if (downedPlayer.GetPlayerIndex() == 0)
        {
            Debug.Log("Looking at player 2");
            if (GameManager.Instance.players[1] != null)
            {
                UIHolder.transform.LookAt(GameManager.Instance.players[1].pCamera.transform);
            }
            else
            {
                Debug.Log(message: "NO at player 2");
            }
        }
        else
        {
            Debug.Log("Looking at player 1");
            if (GameManager.Instance.players[0] != null)
            {
                UIHolder.transform.LookAt(GameManager.Instance.players[0].pCamera.transform);
            }
            else
            {
                Debug.Log(message: "NO at player 1");
            }
        }
    }

    public void SetupReviveInteraction(PlayerManager player)
    {
        downedPlayer = player;
    }
}
