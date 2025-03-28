using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Buttons")]
    [SerializeField] private GameObject masterVolume;
    [SerializeField] private GameObject musicVolume;
    [SerializeField] private GameObject sfxVolume;
    [SerializeField] private GameObject voiceVolume;
    [SerializeField] private GameObject aimSensitivity;
    [SerializeField] private GameObject fullscreen;

    public CanvasGroup buttonGroup;

    [Header("Interactables")]
    //Two copies of the same button/slider, a GameObject for navigation settings, and a Slider/Button for settings specific to those
    [SerializeField] private GameObject masterSlider;
    [SerializeField] private Slider masterSlider2;
    [SerializeField] private GameObject musicSlider;
    [SerializeField] private Slider musicSlider2;
    [SerializeField] private GameObject sfxSlider;
    [SerializeField] private Slider sfxSlider2;
    [SerializeField] private GameObject voiceSlider;
    [SerializeField] private Slider voiceSlider2;
    [SerializeField] private GameObject sensitivitySlider;
    [SerializeField] private Slider sensitivitySlider2;
    [SerializeField] private GameObject fullscreenToggleParent;
    public Button fullscreenToggle;

    [Header("Sprites")]
    public Sprite fullscreenToggleSourceUnchecked;
    [SerializeField] private Sprite fullscreenToggleSelectedUnchecked;
    [SerializeField] private Sprite fullscreenTogglePressedUnchecked;
    public Sprite fullscreenToggleSourceChecked;
    [SerializeField] private Sprite fullscreenToggleSelectedChecked;
    [SerializeField] private Sprite fullscreenTogglePressedChecked;

    private bool isSliderSelected = false;
    private bool canSelect = false;
    private GameObject lastSelected;

    [HideInInspector] public SpriteState uncheckedState;
    [HideInInspector] public SpriteState checkedState;

    private float volume;

    // Start is called before the first frame update
    void Start()
    {
        uncheckedState.highlightedSprite = fullscreenToggleSelectedUnchecked;
        uncheckedState.selectedSprite = fullscreenToggleSelectedUnchecked;
        uncheckedState.pressedSprite = fullscreenTogglePressedUnchecked;
        checkedState.highlightedSprite = fullscreenToggleSelectedChecked;
        checkedState.selectedSprite = fullscreenToggleSelectedChecked;
        checkedState.pressedSprite = fullscreenTogglePressedChecked;

        

        SetSliderDefaults();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (isSliderSelected)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame && canSelect && lastSelected != fullscreen)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(lastSelected);
                Debug.Log("Returning to button");
            }
        }

        if (Screen.fullScreen)
        {
            Debug.Log("Screen is fullscreen");
        }

    }

    public void SetSliderDefaults()
    {
        audioMixer.GetFloat("masterVolume", out volume);
        masterSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("musicVolume", out volume);
        musicSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("sfxVolume", out volume);
        sfxSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("voiceVolume", out volume);
        voiceSlider2.value = Mathf.Pow(10, volume / 20);
        sensitivitySlider2.value = SavedSettings.mouseCamSensitivity;
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

    public void SelectFullscreen()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(fullscreenToggleParent);
        isSliderSelected = true;
        canSelect = false;
        lastSelected = fullscreen;
        StartCoroutine(SelectionCooldown());
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log("Master volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        Debug.Log("Music volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        Debug.Log("SFX volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
    }

    public void SetVoiceVolume(float volume)
    {
        Debug.Log("Voice volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("voiceVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSensitivity(float sensitivity)
    {
        Debug.Log("Cam sensitivity: " + sensitivity + ", " + sensitivity * 10 + ", " + sensitivity * 8);
        
        SavedSettings.mouseCamSensitivity = sensitivity;
        SavedSettings.gamepadHorCamSensitivity = sensitivity * 10;
        SavedSettings.gamepadVertCamSensitivity = sensitivity * 8;
    }

    public void FullscreenToggle()
    {
        if (Screen.fullScreen)
        {
            // turn off fullscreen
            Screen.SetResolution(960, 540, false);
            fullscreenToggle.image.sprite = fullscreenToggleSourceUnchecked;
            fullscreenToggle.spriteState = uncheckedState;
            Debug.Log("Toggling off fullscreen");
        }
        else
        {
            Resolution defaultRes = Screen.currentResolution;
            // turn On fullscreen
            Screen.SetResolution(defaultRes.width, defaultRes.height, true);
            fullscreenToggle.image.sprite = fullscreenToggleSourceChecked;
            fullscreenToggle.spriteState = checkedState;
            Debug.Log("Toggling on fullscreen");
        }
    }

    IEnumerator SelectionCooldown()
    {
        yield return new WaitForSeconds(.1f);
        canSelect = true;
        Debug.Log("Can select slider level");
    }
}
