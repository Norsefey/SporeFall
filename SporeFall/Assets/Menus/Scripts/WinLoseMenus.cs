using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinLoseMenus : MonoBehaviour
{
    //Script that controls win and lose scenes

    //[SerializeField] string level1Name;
    [SerializeField] int gameSceneIndex;

    public void RetryLevel()
    {
        SceneManager.LoadScene(gameSceneIndex);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
