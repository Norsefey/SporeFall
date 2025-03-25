using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PayloadHP : Damageable
{
    [SerializeField] private TMP_Text healthDisplay;
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
        healthDisplay.text = currentHP.ToString("F0") + "/" + maxHP.ToString();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check the health percentage and play the corresponding clip
        float healthPercentage = currentHP / maxHP;

        if (healthPercentage <= 0.75f && !played75)
        {
            PlayAudioClip(health75Clip);
            played75 = true;
        }
        if (healthPercentage <= 0.50f && !played50)
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
