using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinLoseMenus : MonoBehaviour
{
    //Script that controls win and lose scenes

    [SerializeField] string level1Name;


    public void RetryLevel()
    {
        SceneManager.LoadScene(level1Name);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
