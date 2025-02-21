using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TrainHandler;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public WaveManager waveManager;
    public TrainHandler trainHandler;
    public UpgradeManager upgradeManager;
    public GameUIManager gameUI;
    public List<PlayerManager> players = new();
    public PauseMenu pauseMenu;
    public bool paused = false;

    [SerializeField] BackUpPlayerSpawner backUpPlayerSpawner;

    [SerializeField] private float mycelia = 200;
    public float Mycelia { get { return mycelia; } }
    private bool tutorialMycelia = true;

    private void Awake()
    {
        Time.timeScale = 1.0f;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        gameUI.DisplayMycelia(mycelia);
    }
    private void Update()
    {
        if (Tutorial.Instance != null)
        {
            if (Tutorial.Instance.currentScene == "Tutorial" && Tutorial.Instance.tutorialPrompt == 18 && tutorialMycelia == true)
            {
                tutorialMycelia = false;
                IncreaseMycelia(25);
            }
        }
    }
    public void HandlePlayerJoining(PlayerManager player)
    {
        // keep track of active players
        players.Add(player);
        Debug.Log("Player Added");

        player.TogglePControl(false);
        player.TogglePCamera(false);
        player.TogglePVisual(false);

        if(trainHandler != null)
        {
            player.transform.SetParent(trainHandler.transform);

            player.MovePlayerTo(trainHandler.playerSpawnPoint[player.GetPlayerIndex()].position);
            if (trainHandler.trainState == TrainState.Parked)
            {
               trainHandler.DisembarkTrain();
                Debug.Log("Not Moving Disembarking");
            }
        }
        else
        {
            player.transform.SetParent(this.transform);
            SpawnPlayer();
        }

    }
    private void SpawnPlayer()
    {
        if(backUpPlayerSpawner == null)
            return;

        foreach (var player in GameManager.Instance.players)
        {
            StartCoroutine(backUpPlayerSpawner.SpawnPlayer(player));
        }
    }
    public void RemovePlayer(PlayerManager player)
    {
        // remove disconnected players
        players.Remove(player);
    }
    public void IncreaseMycelia(float amount)
    {
        mycelia += amount;
        gameUI.DisplayMycelia(mycelia);
    }
    public void DecreaseMycelia(float amount)
    {
        mycelia -= amount;
        gameUI.DisplayMycelia(mycelia);
    }
    // Called when object is destroyed (including scene changes)
    public void GameOver()
    {
        Debug.Log("No Life No Game");
        // check if all players are dead
        bool allPlayersDead = true;
        foreach (var player in players)
        {
            if(player.pHealth.lives > 0)
                allPlayersDead = false;
        }

        if (allPlayersDead)// if all dead load game over scene
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneTransitioner.Instance.LoadLoseScene();
        }
    }
    private void OnDestroy()
    {
        // Clear static instance if this is the current instance
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
