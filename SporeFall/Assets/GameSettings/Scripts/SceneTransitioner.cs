using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance { get; private set; }
    [SerializeField] private int mainSceneIndex;
    [SerializeField] private int gameSceneIndex;
    [SerializeField] private int winSceneIndex;
    [SerializeField] private int loseSceneIndex;
    [SerializeField] private int survivalGameOverSceneIndex;
    private void Awake()
    {
        Instance = this;
    }
    public void LoadMainMenuScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainSceneIndex);
    }
    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneIndex);
    }
    public void LoadWinScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(winSceneIndex);
    }
    public void LoadLoseScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(loseSceneIndex);
    }
    public void UseIndexToLoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
