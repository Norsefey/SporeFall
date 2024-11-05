using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Script that controls various buttons and screens on main menu

    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject mainScreen;
    [SerializeField] GameObject levelSelectScreen;
    [SerializeField] string level1Name;

    void Start()
    {
        titleScreen.SetActive(true);
        mainScreen.SetActive(false);
        levelSelectScreen.SetActive(false);
    }


    public void ClickToContinue()
    {
        mainScreen.SetActive(true);
        titleScreen.SetActive(false);
    }

    public void LevelSelect()
    {
        levelSelectScreen.SetActive(true);
        mainScreen.SetActive(false);
    }

    public void StartLevel1()
    {
        SceneManager.LoadScene(level1Name);
    }

    public void BackToMain()
    {
        mainScreen.SetActive(true);
        levelSelectScreen.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
