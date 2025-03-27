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
    public SavedSettings savedSettings;

    [Header("Buttons")]
    [SerializeField] private GameObject masterVolume;
    //public Button masterButton;
    [SerializeField] private GameObject musicVolume;
    //public Button musicButton;
    [SerializeField] private GameObject sfxVolume;
    //public Button sfxButton;
    [SerializeField] private GameObject voiceVolume;
    //public Button voiceButton;
    [SerializeField] private GameObject aimSensitivity;
    //public Button aimButton;
    [SerializeField] private GameObject fullscreen;
    //public Button fullscreenButton;

    public CanvasGroup buttonGroup;
    

    [Header("Interactables")]
    [SerializeField] private GameObject masterSlider;
    [SerializeField] private GameObject musicSlider;
    [SerializeField] private GameObject sfxSlider;
    [SerializeField] private GameObject voiceSlider;
    [SerializeField] private GameObject sensitivitySlider;
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

    private float camSensitivity;

    // Start is called before the first frame update
    void Start()
    {
        uncheckedState.highlightedSprite = fullscreenToggleSelectedUnchecked;
        uncheckedState.selectedSprite = fullscreenToggleSelectedUnchecked;
        uncheckedState.pressedSprite = fullscreenTogglePressedUnchecked;
        checkedState.highlightedSprite = fullscreenToggleSelectedChecked;
        checkedState.selectedSprite = fullscreenToggleSelectedChecked;
        checkedState.pressedSprite = fullscreenTogglePressedChecked;
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

    public void SetSensitivity(float sensitivity)
    {
        Debug.Log("Cam sensitivity: " + sensitivity + ", " + sensitivity * 10 + ", " + sensitivity * 8);
        
        savedSettings.mouseCamSensitivity = sensitivity;
        savedSettings.gamepadHorCamSensitivity = sensitivity * 10;
        savedSettings.gamepadVertCamSensitivity = sensitivity * 8;
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
