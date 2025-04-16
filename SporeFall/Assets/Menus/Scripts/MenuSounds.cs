using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSounds : MonoBehaviour
{

    public static MenuSounds Instance;

    [SerializeField] private AudioSource menuAudio;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonPressedSound;
    private bool pressed = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        menuAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayPressedSound()
    {
        pressed = true;
        menuAudio.PlayOneShot(buttonPressedSound);
        StartCoroutine(SoundDelay());
    }

    public void PlayHoverSound()
    {
        if (pressed == false)
        {
            menuAudio.PlayOneShot(buttonHoverSound);
        }
        
    }

    

    IEnumerator SoundDelay()
    {
        yield return new WaitForSeconds(.5f);
        pressed = false;
    }
}
