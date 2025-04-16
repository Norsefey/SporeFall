using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PayloadHP : Damageable
{
    private Payload payload;

    [SerializeField] private AudioClip health75Clip;
    [SerializeField] private AudioClip health50Clip;
    [SerializeField] private AudioClip health25Clip;
    [SerializeField] private AudioClip health0Clip;

    private AudioSource audioSource;

    private bool played75 = false;
    private bool played50 = false;
    private bool played25 = false;
    private bool played0 = false;

    // Start is called before the first frame update
    private void Start()
    {
        currentHP = maxHP; // Initialize health
        audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        currentHP = maxHP; // Initialize health
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // moved it to only check when HP has changed
        PlayHPAudioClip(currentHP / maxHP);
    }
    private void PlayHPAudioClip(float healthPercentage)
    {
        if (healthPercentage <= 0.75f && !played75)
        {
            PlayAudioClip(health75Clip);
            played75 = true;
        }
        else if (healthPercentage <= 0.50f && !played50)
        {
            PlayAudioClip(health50Clip);
            played50 = true;
        }
        if (healthPercentage <= 0.25f && !played25)
        {
            PlayAudioClip(health25Clip);
            played25 = true;
        }
        if (healthPercentage <= 0f && !played0)
        {
            PlayAudioClip(health0Clip);
            played0 = true;
        }
    }
    private void PlayAudioClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    protected override void Die()
    {
        Debug.Log("Payload Destroyed");
        payload.DestroyPayload();
    }
    public void SetManager(Payload payload)
    {
        this.payload = payload;
    }
}
