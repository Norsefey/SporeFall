using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    //Script that controls various buttons and screens on main menu
    //Also handles some settings because the settings menu starts disabled, more controllable via this script

    [SerializeField] SettingsMenu settings;
    public AudioMixer audioMixer;

    [Header("Menus")]
    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject mainScreen;
    [SerializeField] GameObject settingsScreen;
    [SerializeField] GameObject levelSelectScreen;
    [SerializeField] GameObject tutorialQuestionScreen;

    [Header("First Buttons")]
    [SerializeField] GameObject firstTitleButton;
    [SerializeField] GameObject firstMainButton;
    [SerializeField] GameObject firstSettingsButton;
    [SerializeField] GameObject firstTutorialButton;
    [SerializeField] GameObject firstLevelSelectButton;
    private GameObject savedFirstButton;

    [Header("Level Names")]
    [SerializeField] string tutorialName;
    [SerializeField] string level1Name;
    //[SerializeField] string level2Name;
    //[SerializeField] string level3Name;

    [SerializeField] GameObject LoadingScreen;
    [SerializeField] private Slider progressBar;
    private bool isControllerConnected = false;

    [Header("Default Volumes")]
    public float masterVolume;
    [SerializeField] float musicVolume;
    [SerializeField] float enemyVolume;
    [SerializeField] float weaponVolume;
    [SerializeField] float structureVolume;
    [SerializeField] float ambienceVolume;
    [SerializeField] float voiceVolume;

    private float volume;

    void Start()
    {
        SavedSettings.currentLevel = "MainMenu";
        if (SavedSettings.firstOpenedGame)
        {
            //Set audio mixer volumes to given numbers
            Debug.Log("Game first started, setting volumes");

            audioMixer.SetFloat("masterVolume", masterVolume);
            audioMixer.SetFloat("musicVolume", musicVolume);
            audioMixer.SetFloat("enemyVolume", enemyVolume);
            audioMixer.SetFloat("weaponVolume", weaponVolume);
            audioMixer.SetFloat("structureVolume", structureVolume);
            audioMixer.SetFloat("ambienceVolume", ambienceVolume);
            audioMixer.SetFloat("voiceVolume", voiceVolume);
            SavedSettings.firstOpenedGame = false;
        }

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

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;

        titleScreen.SetActive(true);
        mainScreen.SetActive(false);
        levelSelectScreen.SetActive(false);

        Debug.Log("There are " + InputSystem.devices.Count + "devices connected");
        foreach (var device in InputSystem.devices)
        {
            Debug.Log(device.name);
        }

        savedFirstButton = firstTitleButton;
        InputSystem.onDeviceChange += OnDeviceChange;

        if (Gamepad.all.Count > 0)
        {
            //If a controller is being used, highlights the first button
            //If a controller is not being used, no buttons are highlighted
            isControllerConnected = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(savedFirstButton);
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Update()
    {

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            if (Screen.fullScreen)
            {
                // turn off fullscreen
                Screen.SetResolution(960, 540, false);
                settings.fullscreenToggle.image.sprite = settings.fullscreenToggleSourceUnchecked;
                settings.fullscreenToggle.spriteState = settings.uncheckedState;
                Debug.Log("Disabling fullscreen");
            }
            else
            {
                Resolution defaultRes = Screen.currentResolution;
                // turn On fullscreen
                Screen.SetResolution(defaultRes.width, defaultRes.height, true);
                settings.fullscreenToggle.image.sprite = settings.fullscreenToggleSourceChecked;
                settings.fullscreenToggle.spriteState = settings.checkedState;
                Debug.Log("Enabling fullscreen");
            }
        }
    }
    
    public void ClickToContinue()
    {
        mainScreen.SetActive(true);
        titleScreen.SetActive(false);
        if (isControllerConnected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainButton);
        }
        savedFirstButton = firstMainButton;
    }

    public void OpenSettingsMenu()
    {
        settingsScreen.SetActive(true);
        mainScreen.SetActive(false);
        if (isControllerConnected)
        {
            settings.buttons.SetActive(true);
            Debug.Log("Controller detected, enabling buttons");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton);
        }
        else if (!isControllerConnected)
        {
            settings.buttons.SetActive(false);
            Debug.Log("Controller not detected, disabling buttons");
            EventSystem.current.SetSelectedGameObject(null);
        }
        savedFirstButton = firstSettingsButton;
    }

    public void TutorialQuestion()
    {
        if (SavedSettings.firstTutorialQuestion)
        {
            tutorialQuestionScreen.SetActive(true);
            mainScreen.SetActive(false);
            if (isControllerConnected)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstTutorialButton);
            }
            savedFirstButton = firstTutorialButton;
            SavedSettings.firstTutorialQuestion = false;
        }

        else
        {
            LevelSelect();
        }
        
    }

    public void LevelSelect()
    {
        levelSelectScreen.SetActive(true);
        tutorialQuestionScreen.SetActive(false);
        mainScreen.SetActive(false);
        if (isControllerConnected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstLevelSelectButton);
        }
        savedFirstButton = firstLevelSelectButton;
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene(tutorialName);
        SavedSettings.currentLevel = "Tutorial";
    }
    public void SetDifficulty(bool easyMode)
    {
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.SetEasyMode(easyMode);
            Debug.Log("Easy Mode: " + easyMode);
        }
    }
    public void StartLevel(int index)
    {
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync(index));

        if (index == 1)
        {
            SavedSettings.currentLevel = "GlowingForest";
        }

        else if (index == 7)
        {
            SavedSettings.currentLevel = "ToxicSwamp";
        }
    }

    public void BackToMain()
    {
        mainScreen.SetActive(true);
        levelSelectScreen.SetActive(false);
        settingsScreen.SetActive(false);
        if (isControllerConnected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainButton);
        }
        savedFirstButton = firstMainButton;
    }

    public void LoadSceneUsingIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadSceneAsync(int index)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            //Debug.Log(operation.progress);
            // Update progress bar (normalized 0 to 1)
            progressBar.value = Mathf.Clamp01(operation.progress / 0.9f);

            if (operation.progress >= 0.9f)
            {
                // Wait for user input or small delay before activating
                yield return new WaitForSeconds(1f);
                Debug.Log("Activating Scene");
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device Connected: {device.displayName}");
            isControllerConnected = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(savedFirstButton);
            if (savedFirstButton = firstSettingsButton)
            {
                settings.buttons.SetActive(true);
            }
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");
            isControllerConnected= false;
            EventSystem.current.SetSelectedGameObject(null);

            if (savedFirstButton = firstSettingsButton)
            {
                settings.buttons.SetActive(false);
            }
        }
    }
}
