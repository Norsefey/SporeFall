using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEffects : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private GameObject impactVFXPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip impactSound;
    [Range(0f, 1f)][SerializeField] private float soundVolume = 0.5f;

    public void Initialize(GameObject vfxPrefab = null, AudioClip soundClip = null)
    {
        if (vfxPrefab != null)
            impactVFXPrefab = vfxPrefab;

        if (soundClip != null)
            impactSound = soundClip;

        if (audioSource != null)
            audioSource.volume = soundVolume;
    }

    public void PlayImpactEffects(Vector3 position)
    {
        // Play impact sound
        PlayImpactSound();

        // Create impact visual effect
        SpawnImpactVFX(position);
    }

    private void PlayImpactSound()
    {
        if (audioSource != null && impactSound != null && audioSource.isActiveAndEnabled)
        {
            audioSource.PlayOneShot(impactSound, soundVolume);
        }
    }

    private void SpawnImpactVFX(Vector3 position)
    {
        if (impactVFXPrefab != null)
        {
            // Try to get VFX from pool
            if (PoolManager.Instance != null &&
                PoolManager.Instance.vfxPool.TryGetValue(impactVFXPrefab, out VFXPool pool))
            {
                VFXPoolingBehavior vfx = pool.Get(position, transform.rotation);
                vfx.Initialize(pool);
            }
            else
            {
                // Fallback to instantiate
                Instantiate(impactVFXPrefab, position, transform.rotation);
            }
        }
    }
}
