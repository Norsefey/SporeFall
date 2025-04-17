using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSounds : MonoBehaviour
{

    public static MenuSounds Instance;

    [SerializeField] private AudioSource hoverAudio;
    [SerializeField] private AudioSource pressedAudio;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonPressedSound;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayPressedSound()
    {
        pressedAudio.PlayOneShot(buttonPressedSound);
        hoverAudio.enabled = false;
        StartCoroutine(SoundDelay());
    }

    public void PlayHoverSound()
    {
        if (hoverAudio.isActiveAndEnabled)
        {
            hoverAudio.PlayOneShot(buttonHoverSound);
        } 
    }

    

    IEnumerator SoundDelay()
    {
        yield return new WaitForSecondsRealtime(.5f);
        hoverAudio.enabled = true;
    }
}
