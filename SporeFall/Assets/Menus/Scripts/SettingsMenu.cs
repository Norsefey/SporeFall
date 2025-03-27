using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Buttons")]
    [SerializeField] private GameObject masterVolume;
    public Button masterButton;
    [SerializeField] private GameObject musicVolume;
    public Button musicButton;
    [SerializeField] private GameObject sfxVolume;
    public Button sfxButton;
    [SerializeField] private GameObject voiceVolume;
    public Button voiceButton;
    [SerializeField] private GameObject aimSensitivity;
    public Button aimButton;

    public CanvasGroup buttonGroup;
    

    [Header("Sliders")]
    [SerializeField] private GameObject masterSlider;
    [SerializeField] private GameObject musicSlider;
    [SerializeField] private GameObject sfxSlider;
    [SerializeField] private GameObject voiceSlider;
    [SerializeField] private GameObject sensitivitySlider;

    private bool isSliderSelected = false;
    private bool canSelect = false;
    private GameObject lastSelected;
    private bool isGamepadConnected = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (isSliderSelected)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame && canSelect)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(lastSelected);
                Debug.Log("Returning to button");
            }
        }

    }

    public void SelectMasterVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(masterSlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = masterVolume;
        StartCoroutine(SelectionCooldown());
        Debug.Log("Master volume selected");
    }

    public void SelectMusicVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(musicSlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = musicVolume;
        StartCoroutine(SelectionCooldown());
    }

    public void SelectSFXVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sfxSlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = sfxVolume;
        StartCoroutine(SelectionCooldown());
    }

    public void SelectAimSensitivity()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sensitivitySlider);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = aimSensitivity;
        StartCoroutine(SelectionCooldown());
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log("Master volume: " + volume);
        audioMixer.SetFloat("masterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        Debug.Log("Music volume: " + volume);
        audioMixer.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        Debug.Log("SFX volume: " + volume);
        audioMixer.SetFloat("sfxVolume", volume);
    }

    public void SetVoiceVolume(float volume)
    {
        Debug.Log("Voice volume: " + volume);
        audioMixer.SetFloat("voiceVolume", volume);
    }

    IEnumerator SelectionCooldown()
    {
        yield return new WaitForSeconds(.1f);
        canSelect = true;
        Debug.Log("Can select slider level");
    }
}
