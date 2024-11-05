using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    //Script to control pause menu


    [SerializeField] GameObject defaultScreen;
    [SerializeField] GameObject controlsScreen;
    [SerializeField] GameObject mainConfirmScreen;
    [SerializeField] GameObject retryConfirmScreen;
    [SerializeField] GameObject quitConfirmScreen;
    [SerializeField] string level1Name;

    // Start is called before the first frame update
    void Start()
    {
        defaultScreen.SetActive(true);
        controlsScreen.SetActive(false);
        mainConfirmScreen.SetActive(false);
        retryConfirmScreen.SetActive(false);
        quitConfirmScreen.SetActive(false);
    }


    public void OpenControlsMenu()
    {
        controlsScreen.SetActive(true);
        defaultScreen.SetActive(false);
    }

    public void BackFromControls()
    {
        defaultScreen.SetActive(true);
        controlsScreen.SetActive(false);
    }

    public void MainMenuConfirmScreen()
    {
        mainConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
    }

    public void BackFromMainConfirm()
    {
        defaultScreen.SetActive(true);
        mainConfirmScreen.SetActive(false);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RetryConfirmScreen()
    {
        retryConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
    }

    public void BackFromRetry()
    {
        defaultScreen.SetActive(true);
        retryConfirmScreen.SetActive(false);
    }

    public void Retry()
    {
        SceneManager.LoadScene(level1Name);
    }

    public void QuitConfirmScreen()
    {
        quitConfirmScreen.SetActive(true);
        defaultScreen.SetActive(false);
    }

    public void BackFromQuitConfirm()
    {
        defaultScreen.SetActive(true);
        quitConfirmScreen.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
