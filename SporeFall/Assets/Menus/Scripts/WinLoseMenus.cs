using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class WinLoseMenus : MonoBehaviour
{
    //Script that controls win and lose scenes

    //[SerializeField] string level1Name;
    [SerializeField] int gameSceneIndex;
    [SerializeField] GameObject firstButton;

    [SerializeField] TMP_Text completionText;

    private void Start()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        if (Gamepad.all.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
        if (PersistentGameManager.Instance != null)
        {
            int minutes = Mathf.FloorToInt(PersistentGameManager.Instance.completionTime / 60F);
            int seconds = Mathf.FloorToInt(PersistentGameManager.Instance.completionTime - minutes * 60);
            completionText.text = "Level Completion Time: " + string.Format("{0:0}:{1:00}", minutes, seconds);
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
                Screen.SetResolution(1280, 720, false);
            }
            else
            {
                Resolution defaultRes = Screen.currentResolution;
                // turn On fullscreen
                Screen.SetResolution(defaultRes.width, defaultRes.height, true);
            }
        }
    }


    public void RetryLevel()
    {
        SceneManager.LoadScene(SavedSettings.currentLevel);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device Connected: {device.displayName}");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
