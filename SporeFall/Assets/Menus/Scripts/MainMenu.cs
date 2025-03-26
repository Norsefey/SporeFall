using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    //Script that controls various buttons and screens on main menu and allows player to fullscreen

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

    [Header("Level Names")]
    [SerializeField] string tutorialName;
    [SerializeField] string level1Name;
    //[SerializeField] string level2Name;
    //[SerializeField] string level3Name;

    [SerializeField] GameObject LoadingScreen;
    [SerializeField] private Slider progressBar;


    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;

        titleScreen.SetActive(true);
        mainScreen.SetActive(false);
        levelSelectScreen.SetActive(false);

        Debug.Log("There is/are " + Gamepad.all.Count + "Gamepad(s) connected");

        if (Gamepad.all.Count > 0 )
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstTitleButton);
        }

        InputSystem.onDeviceChange += OnDeviceChange;
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
            }
            else
            {
                Resolution defaultRes = Screen.currentResolution;
                // turn On fullscreen
                Screen.SetResolution(defaultRes.width, defaultRes.height, true);
            }
        }
    }
    
    public void ClickToContinue()
    {
        mainScreen.SetActive(true);
        titleScreen.SetActive(false);
        if (Gamepad.all.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainButton);
        }
    }

    public void OpenSettingsMenu()
    {
        settingsScreen.SetActive(true);
        mainScreen.SetActive(false);
        if (Gamepad.all.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton);
        }
        
    }

    public void TutorialQuestion()
    {
        tutorialQuestionScreen.SetActive(true);
        mainScreen.SetActive(false);
        if (Gamepad.all.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstTutorialButton);
        }
        
    }

    public void LevelSelect()
    {
        levelSelectScreen.SetActive(true);
        tutorialQuestionScreen.SetActive(false);
        if (Gamepad.all.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstLevelSelectButton);
        }
        
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
        if (Gamepad.all.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainButton);
        }
        
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

        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");

        }
    }
}
