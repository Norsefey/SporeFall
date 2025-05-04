using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleHP : Damageable
{
    [SerializeField] private TentaclePart tentacle;
    [SerializeField] private AudioClip hitSound;
    private AudioSource audioSource;

    private void Awake()
    {
        if (tentacle == null)
            tentacle = GetComponent<TentaclePart>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && hitSound != null)
        {
            // Create audio source if needed
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    public override void TakeDamage(float damage)
    {
        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        // Apply damage
        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        if (tentacle != null)
        {
            tentacle.DestroyTentacle();
        }
        else
        {
            Debug.LogError("TentacleHP has no reference to TentaclePart!");
            gameObject.SetActive(false);
        }
    }
}
