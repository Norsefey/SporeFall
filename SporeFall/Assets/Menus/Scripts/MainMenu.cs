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
    private float volume = 2;
    private float volume2;


    void Start()
    {
        audioMixer.SetFloat("masterVolume", volume);
        audioMixer.SetFloat("musicVolume", volume);
        audioMixer.SetFloat("sfxVolume", volume);
        audioMixer.SetFloat("voiceVolume", volume);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;

        titleScreen.SetActive(true);
        mainScreen.SetActive(false);
        levelSelectScreen.SetActive(false);

        Debug.Log("There are " + InputSystem.devices.Count + "devices connected");
        savedFirstButton = firstTitleButton;
        InputSystem.onDeviceChange += OnDeviceChange;
        if (InputSystem.devices.Count > 2 )
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
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
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
            settings.buttonGroup.interactable = true;
            Debug.Log("Controller detected, enabling buttons");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton);
        }
        else if (!isControllerConnected)
        {
            settings.buttonGroup.interactable = false;
            Debug.Log("Controller not detected, disabling buttons");
            EventSystem.current.SetSelectedGameObject(null);
        }
        savedFirstButton = firstSettingsButton;
    }

    public void TutorialQuestion()
    {
        tutorialQuestionScreen.SetActive(true);
        mainScreen.SetActive(false);
        if (isControllerConnected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstTutorialButton);
        }
        savedFirstButton = firstTutorialButton;
    }

    public void LevelSelect()
    {
        levelSelectScreen.SetActive(true);
        tutorialQuestionScreen.SetActive(false);
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
    }

    public void StartLevel1()
    {
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync(1));
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
            Debug.Log(operation.progress);
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
                settings.buttonGroup.interactable = true;
            }
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");
            isControllerConnected= false;
            EventSystem.current.SetSelectedGameObject(null);

            if (savedFirstButton = firstSettingsButton)
            {
                settings.buttonGroup.interactable = false;
            }
        }
    }
}
