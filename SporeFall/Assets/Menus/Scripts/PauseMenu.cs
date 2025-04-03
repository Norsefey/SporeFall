using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Security.Cryptography;
using System.Security.Authentication.ExtendedProtection;

public class PauseMenu : MonoBehaviour
{
    //Script to control pause menu

    [SerializeField] string level1Name;
    [SerializeField] SettingsMenu settings;
    public AudioMixer audioMixer;

    [Header("Menus")]
    [SerializeField] GameObject pauseMenuBG;
    [SerializeField] GameObject defaultScreen;
    [SerializeField] GameObject settingsScreen;
    [SerializeField] GameObject chooseControls;
    [SerializeField] GameObject keyboardControls;
    [SerializeField] GameObject xboxControls;
    [SerializeField] GameObject playstationControls;
    [SerializeField] GameObject controlsBackButton1;
    [SerializeField] GameObject controlsBackButton2;
    [SerializeField] GameObject mainConfirmScreen;
    [SerializeField] GameObject retryConfirmScreen;
    [SerializeField] GameObject quitConfirmScreen;

    [Header("First Buttons")]
    [SerializeField] GameObject firstPausedButton;
    [SerializeField] GameObject firstSettingsButton;
    [SerializeField] GameObject firstControlsButton;
    [SerializeField] GameObject firstMainMenuButton;
    [SerializeField] GameObject firstRetryButton;
    [SerializeField] GameObject firstQuitButton;

    private GameObject savedFirstButton;
    private GameObject savedActiveScreen;

    private bool isControllerConnected = false;
    private bool isPaused = false;
    private float volume;

    void Start()
    {
        settings.buttonGroup.interactable = false;
        savedFirstButton = firstPausedButton;

        InputSystem.onDeviceChange += OnDeviceChange;

        //Set default slider values to match audio volumes
        audioMixer.GetFloat("masterVolume", out volume);
        settings.masterSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("musicVolume", out volume);
        settings.musicSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("enemyVolume", out volume);
        settings.enemySlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("weaponVolume", out volume);
        settings.weaponSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("structureVolume", out volume);
        settings.structureSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("ambienceVolume", out volume);
        settings.ambienceSlider2.value = Mathf.Pow(10, volume / 20);
        audioMixer.GetFloat("voiceVolume", out volume);
        settings.voiceSlider2.value = Mathf.Pow(10, volume / 20);

        //Set default sensitivity slider values
        settings.sensitivityP1Slider2.value = SavedSettings.mouseCamSensitivity;
        settings.sensitivityP2Slider2.value = SavedSettings.mouseCamSensitivity2;
    }

    private void Update()
    {
        // Check for gamepad input
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (settings.isSliderSelected == false && isPaused && savedActiveScreen != defaultScreen)
        {
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                if (savedActiveScreen == keyboardControls || savedActiveScreen == xboxControls || savedActiveScreen == playstationControls)
                {
                    chooseControls.SetActive(true);
                    savedActiveScreen.SetActive(false);
                    controlsBackButton1.SetActive(true);
                    controlsBackButton2.SetActive(false);
                    savedActiveScreen = chooseControls;
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(firstControlsButton);
                    savedFirstButton = firstControlsButton;
                    Debug.Log("Returning to controls menu");
                }

                else
                {
                    defaultScreen.SetActive(true);
                    savedActiveScreen.SetActive(false);
                    controlsBackButton1.SetActive(false);
                    savedActiveScreen = defaultScreen;
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(firstPausedButton);
                    savedFirstButton = firstPausedButton;
                    Debug.Log("Returning to default menu");
                }
                
            }
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public void OpenPauseMenu()
    {
        pauseMenuBG.SetActive(true);
        defaultScreen.SetActive(true);
        settingsScreen.SetActive(false);
        chooseControls.SetActive(false);
        keyboardControls.SetActive(false);
        xboxControls.SetActive(false);
        playstationControls.SetActive(false);
        controlsBackButton1.SetActive(false);
        controlsBackButton2.SetActive(false);
        mainConfirmScreen.SetActive(false);
        retryConfirmScreen.SetActive(false);
        quitConfirmScreen.SetActive(false);
        if (Gamepad.all.Count > 0)
        {
            //If a controller is being used, highlights the first button
            //If a controller is not being used, no buttons are highlighted
            isControllerConnected = true;
            isPaused = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(savedFirstButton);
        }
        savedActiveScreen = defaultScreen;
    }
    public void ClosePauseMenu()
    {
        pauseMenuBG.SetActive(false);
        defaultScreen.SetActive(false);
        settingsScreen.SetActive(false);
        chooseControls.SetActive(false);
        keyboardControls.SetActive(false);
        xboxControls.SetActive(false);
        playstationControls.SetActive(false);
        controlsBackButton1.SetActive(false);
        controlsBackButton2.SetActive(false);
        mainConfirmScreen.SetActive(false);
        retryConfirmScreen.SetActive(false);
        quitConfirmScreen.SetActive(false);
    }

    public void Back()
    {

        if (savedActiveScreen == keyboardControls || savedActiveScreen == xboxControls || savedActiveScreen == playstationControls)
        {
            chooseControls.SetActive(true);
            savedActiveScreen.SetActive(false);
            controlsBackButton1.SetActive(true);
            controlsBackButton2.SetActive(false);
            savedActiveScreen = chooseControls;

            if (isControllerConnected && isPaused)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstControlsButton);
                savedFirstButton = firstControlsButton;
            }
        }

        else
        {
            defaultScreen.SetActive(true);
            savedActiveScreen.SetActive(false);
            controlsBackButton1.SetActive(false);

            if (isControllerConnected && isPaused)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstPausedButton);
                savedFirstButton = firstPausedButton;
            }
        }
    }

    public void OpenSettingsMenu()
    {
        settingsScreen.SetActive(true);
        defaultScreen.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            settings.buttonGroup.interactable = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton);
        }
        savedFirstButton = firstSettingsButton;
        savedActiveScreen = settingsScreen;
    }

    public void OpenControlsMenu()
    {
        chooseControls.SetActive(true);
        controlsBackButton1.SetActive(true);
        defaultScreen.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstControlsButton);
        }
        savedFirstButton = firstControlsButton;
        savedActiveScreen = chooseControls;
    }

    public void OpenKeyboardControls()
    {
        keyboardControls.SetActive(true);
        chooseControls.SetActive(false);
        controlsBackButton2.SetActive(true);
        controlsBackButton1.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(controlsBackButton2);
        }
        savedFirstButton = controlsBackButton2;
        savedActiveScreen = keyboardControls;
    }

    public void OpenXboxControls()
    {
        xboxControls.SetActive(true);
        chooseControls.SetActive(false);
        controlsBackButton2.SetActive(true);
        controlsBackButton1.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(controlsBackButton2);
        }
        savedFirstButton = controlsBackButton2;
        savedActiveScreen = xboxControls;
    }

    public void OpenPlaystationControls()
    {
        playstationControls.SetActive(true);
        chooseControls.SetActive(false);
        controlsBackButton2.SetActive(true);
        controlsBackButton1.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(controlsBackButton2);
        }
        savedFirstButton = controlsBackButton2;
        savedActiveScreen = playstationControls;
    }

    public void MainMenuConfirmScreen()
    {
        mainConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainMenuButton);
        }
        savedFirstButton = firstMainMenuButton;
        savedActiveScreen = mainConfirmScreen;
    }

    public void GoToMainMenu()
    {
        SceneTransitioner.Instance.LoadMainMenuScene();
    }

    public void RetryConfirmScreen()
    {
        retryConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstRetryButton);
        }
        savedFirstButton = firstRetryButton;
        savedActiveScreen = retryConfirmScreen;
    }

    public void Retry()
    {
        SceneTransitioner.Instance.LoadGameScene();
    }

    public void QuitConfirmScreen()
    {
        quitConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
        if (isControllerConnected && isPaused)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstQuitButton);
        }
        savedFirstButton = firstQuitButton;
        savedActiveScreen = quitConfirmScreen;
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added && isPaused)
        {
            Debug.Log($"Device Connected: {device.displayName}");
            isControllerConnected = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(savedFirstButton);
            if (savedFirstButton = firstSettingsButton)
            {
                settings.buttonGroup.interactable = true;
            }
        }
        else if (change == InputDeviceChange.Disconnected && isPaused)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");
            isControllerConnected = false;
            EventSystem.current.SetSelectedGameObject(null);

            if (savedFirstButton = firstSettingsButton)
            {
                settings.buttonGroup.interactable = false;
            }
        }
    }
}
