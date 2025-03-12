using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    //Script to control pause menu

    [SerializeField] string level1Name;

    [Header("Menus")]
    [SerializeField] GameObject defaultScreen;
    [SerializeField] GameObject controlsScreen;
    [SerializeField] GameObject mainConfirmScreen;
    [SerializeField] GameObject retryConfirmScreen;
    [SerializeField] GameObject quitConfirmScreen;

    [Header("First Buttons")]
    [SerializeField] GameObject firstPausedButton;
    [SerializeField] GameObject firstControlsButton;
    [SerializeField] GameObject firstMainMenuButton;
    [SerializeField] GameObject firstRetryButton;
    [SerializeField] GameObject firstQuitButton;


    public void OpenPauseMenu()
    {
        defaultScreen.SetActive(true);
        controlsScreen.SetActive(false);
        mainConfirmScreen.SetActive(false);
        retryConfirmScreen.SetActive(false);
        quitConfirmScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPausedButton);
    }
    public void ClosePauseMenu()
    {
        defaultScreen.SetActive(false);
        controlsScreen.SetActive(false);
        mainConfirmScreen.SetActive(false);
        retryConfirmScreen.SetActive(false);
        quitConfirmScreen.SetActive(false);
    }

    public void OpenControlsMenu()
    {
        controlsScreen.SetActive(true);
        defaultScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstControlsButton);
    }

    public void BackFromControls()
    {
        defaultScreen.SetActive(true);
        controlsScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPausedButton);
    }

    public void MainMenuConfirmScreen()
    {
        mainConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuButton);
    }

    public void BackFromMainConfirm()
    {
        defaultScreen.SetActive(true);
        mainConfirmScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPausedButton);
    }

    public void GoToMainMenu()
    {
        SceneTransitioner.Instance.LoadMainMenuScene();
    }

    public void RetryConfirmScreen()
    {
        retryConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstRetryButton);
    }

    public void BackFromRetry()
    {
        defaultScreen.SetActive(true);
        retryConfirmScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPausedButton);
    }

    public void Retry()
    {
        SceneTransitioner.Instance.LoadGameScene();
    }

    public void QuitConfirmScreen()
    {
        quitConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstQuitButton);
    }

    public void BackFromQuitConfirm()
    {
        defaultScreen.SetActive(true);
        quitConfirmScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPausedButton);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
