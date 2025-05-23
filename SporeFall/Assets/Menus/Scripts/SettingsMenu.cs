using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Buttons")]
    [SerializeField] private GameObject masterVolume;
    [SerializeField] private GameObject musicVolume;
    [SerializeField] private GameObject enemyVolume;
    [SerializeField] private GameObject weaponVolume;
    [SerializeField] private GameObject structureVolume;
    [SerializeField] private GameObject ambienceVolume;
    [SerializeField] private GameObject voiceVolume;
    [SerializeField] private GameObject aimSensitivity;
    [SerializeField] private GameObject aimSensitivity2;
    [SerializeField] private GameObject fullscreen;

    public GameObject buttons;
    public GameObject noGamepadText;

    [Header("Interactables")]
    //Two copies of the same button/slider, a GameObject for navigation settings, and a Slider/Button for settings specific to those
    [SerializeField] private GameObject masterSlider;
    public Slider masterSlider2;
    [SerializeField] private GameObject musicSlider;
    public Slider musicSlider2;
    [SerializeField] private GameObject enemySlider;
    public Slider enemySlider2;
    [SerializeField] private GameObject weaponSlider;
    public Slider weaponSlider2;
    [SerializeField] private GameObject structureSlider;
    public Slider structureSlider2;
    [SerializeField] private GameObject ambienceSlider;
    public Slider ambienceSlider2;
    [SerializeField] private GameObject voiceSlider;
    public Slider voiceSlider2;
    [SerializeField] private GameObject sensitivityP1Slider;
    public Slider sensitivityP1Slider2;
    [SerializeField] private GameObject sensitivityP2Slider;
    public Slider sensitivityP2Slider2;
    [SerializeField] private GameObject fullscreenToggleParent;
    public Button fullscreenToggle;

    [Header("Sprites")]
    public Sprite fullscreenToggleSourceUnchecked;
    [SerializeField] private Sprite fullscreenToggleSelectedUnchecked;
    [SerializeField] private Sprite fullscreenTogglePressedUnchecked;
    public Sprite fullscreenToggleSourceChecked;
    [SerializeField] private Sprite fullscreenToggleSelectedChecked;
    [SerializeField] private Sprite fullscreenTogglePressedChecked;

    [HideInInspector] public bool isSliderSelected = false;
    private GameObject lastSelected;

    [HideInInspector] public SpriteState uncheckedState;
    [HideInInspector] public SpriteState checkedState;

    // Start is called before the first frame update
    void Start()
    {
        uncheckedState.highlightedSprite = fullscreenToggleSelectedUnchecked;
        uncheckedState.selectedSprite = fullscreenToggleSelectedUnchecked;
        uncheckedState.pressedSprite = fullscreenTogglePressedUnchecked;
        checkedState.highlightedSprite = fullscreenToggleSelectedChecked;
        checkedState.selectedSprite = fullscreenToggleSelectedChecked;
        checkedState.pressedSprite = fullscreenTogglePressedChecked;

        if (Screen.fullScreen)
        {
            fullscreenToggle.image.sprite = fullscreenToggleSourceChecked;
            fullscreenToggle.spriteState = checkedState;
        }
        else
        {
            fullscreenToggle.image.sprite = fullscreenToggleSourceUnchecked;
            fullscreenToggle.spriteState = uncheckedState;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (isSliderSelected)
        {
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(lastSelected);
                isSliderSelected = false;
                if (MenuSounds.Instance != null)
                {
                    MenuSounds.Instance.PlayPressedSound();
                }
                //Debug.Log("Returning to button");
            }
        }

    }

    #region Button Selection
    public void SelectMasterVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(masterSlider);
        isSliderSelected = true;
        lastSelected = masterVolume;
        Debug.Log("Master volume selected");
    }

    public void SelectMusicVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(musicSlider);
        isSliderSelected = true;
        lastSelected = musicVolume;
    }

    public void SelectEnemyVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(enemySlider);
        isSliderSelected = true;
        lastSelected = enemyVolume;
    }

    public void SelectWeaponVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(weaponSlider);
        isSliderSelected = true;
        lastSelected = weaponVolume;
    }

    public void SelectStructureVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(structureSlider);
        isSliderSelected = true;
        lastSelected = structureVolume;
    }

    public void SelectAmbienceVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(ambienceSlider);
        isSliderSelected = true;
        lastSelected = ambienceVolume;
    }

    public void SelectVoiceVolume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(voiceSlider);
        isSliderSelected = true;
        lastSelected = voiceVolume;
    }

    public void SelectAimSensitivity()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sensitivityP1Slider);
        isSliderSelected = true;
        lastSelected = aimSensitivity;
    }

    public void SelectAimSensitivity2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sensitivityP2Slider);
        isSliderSelected = true;
        lastSelected = aimSensitivity2;
    }

    public void SelectFullscreen()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(fullscreenToggleParent);
        isSliderSelected = true;
        lastSelected = fullscreen;
    }

    #endregion

    #region Confirm Settings
    public void SetMasterVolume(float volume)
    {
        //Using this math rather than flat numbers so the volume scale is linear rather than exponential
        //Debug.Log("Master volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        //Debug.Log("Music volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetEnemySFXVolume(float volume)
    {
        //Debug.Log("Enemy SFX volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("enemyVolume", Mathf.Log10(volume) * 20);
    }

    public void SetWeaponSFXVolume(float volume)
    {
        //Debug.Log("Weapon SFX volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("weaponVolume", Mathf.Log10(volume) * 20);
    }

    public void SetStructureSFXVolume(float volume)
    {
        //Debug.Log("Structure SFX volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("structureVolume", Mathf.Log10(volume) * 20);
    }

    public void SetAmbienceSFXVolume(float volume)
    {
        //Debug.Log("Ambience SFX volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("ambienceVolume", Mathf.Log10(volume) * 20);
    }

    public void SetVoiceVolume(float volume)
    {
        //Debug.Log("Voice volume: " + Mathf.Log10(volume) * 20);
        audioMixer.SetFloat("voiceVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSensitivity(float sensitivity)
    {
        //Debug.Log("Cam sensitivity: " + sensitivity + ", " + sensitivity * 10 + ", " + sensitivity * 8);
        
        SavedSettings.mouseCamSensitivity = sensitivity;
        SavedSettings.gamepadHorCamSensitivity = sensitivity * 10;
        SavedSettings.gamepadVertCamSensitivity = sensitivity * 8;

        if (SavedSettings.currentLevel != "MainMenu")
        {
            GameManager.Instance.UpdatePlayerSensitivity(0);
        }
    }

    public void SetSensitivity2(float sensitivity)
    {
        //Debug.Log("Cam sensitivity: " + sensitivity + ", " + sensitivity * 10 + ", " + sensitivity * 8);

        SavedSettings.mouseCamSensitivity2 = sensitivity;
        SavedSettings.gamepadHorCamSensitivity2 = sensitivity * 10;
        SavedSettings.gamepadVertCamSensitivity2 = sensitivity * 8;

        if (SavedSettings.currentLevel != "MainMenu" && GameManager.Instance.players.Count > 1)
        {
            GameManager.Instance.UpdatePlayerSensitivity(1);
        }
    }

    public void FullscreenToggle()
    {
        if (Screen.fullScreen)
        {
            // turn off fullscreen
            Screen.SetResolution(960, 540, false);
            fullscreenToggle.image.sprite = fullscreenToggleSourceUnchecked;
            fullscreenToggle.spriteState = uncheckedState;
            //Debug.Log("Toggling off fullscreen");
        }
        else
        {
            Resolution defaultRes = Screen.currentResolution;
            // turn On fullscreen
            Screen.SetResolution(defaultRes.width, defaultRes.height, true);
            fullscreenToggle.image.sprite = fullscreenToggleSourceChecked;
            fullscreenToggle.spriteState = checkedState;
            //Debug.Log("Toggling on fullscreen");
        }
    }

    #endregion


}
