using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoHP : Damageable
{
    [SerializeField] private OctoBoss mainBody;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip shieldedHitSound;
    [SerializeField] private Material vulnerableMaterial;
    [SerializeField] private Material shieldedMaterial;
    [SerializeField] private Renderer bodyRenderer;

    private bool wasVulnerable = false;
    private AudioSource audioSource;

    private void Awake()
    {
        if (mainBody == null)
            mainBody = GetComponent<OctoBoss>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hitSound != null || shieldedHitSound != null))
        {
            // Create audio source if needed
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    private void UpdateVisuals(bool isVulnerable)
    {
        // Change material based on vulnerability state
        if (bodyRenderer != null && vulnerableMaterial != null && shieldedMaterial != null)
        {
            bodyRenderer.material = isVulnerable ? vulnerableMaterial : shieldedMaterial;
        }
    }

    public override void TakeDamage(float damage)
    {
        // Apply damage multiplier based on vulnerability
        float damageMultiplier = mainBody.CalculateDamageMultiplier();
        float modifiedDamage = damage * damageMultiplier;

        if (audioSource != null)
        {
            if (damageMultiplier < 1.0f && shieldedHitSound != null)
                audioSource.PlayOneShot(shieldedHitSound);
            else if (hitSound != null)
                audioSource.PlayOneShot(hitSound);
        }

        // Apply damage
        base.TakeDamage(modifiedDamage);

        // Log damage reduction if applicable
        if (damageMultiplier < 1.0f)
        {
            Debug.Log($"Damage reduced: {damage} → {modifiedDamage} (Multiplier: {damageMultiplier})");
        }
        else if (damageMultiplier > 1.0f)
        {
            Debug.Log($"Damage increased: {damage} → {modifiedDamage} (Multiplier: {damageMultiplier})");
        }
    }

    protected override void Die()
    {
        if (mainBody != null)
        {
            mainBody.Die();
        }
        else
        {
            Debug.LogError("TentacleBodyHP has no reference to TentacleEnemy!");
            gameObject.SetActive(false);
        }
    }
}
