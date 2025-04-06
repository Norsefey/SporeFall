using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TrainHandler;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("References")]
    public WaveManager waveManager;
    public TrainHandler trainHandler;
    public UpgradeManager upgradeManager;
    public GameUIManager gameUI;
    
    [Header("Pause Menu")]
    public PauseMenu pauseMenu;
    public bool paused = false;

    [Header("Build System")]
    public List<GameObject> availableStructures;
    [SerializeField] private float mycelia = 200;
    public float Mycelia { get { return mycelia; } }
    private bool tutorialMycelia = true;

    [Header("Coop Manager")]
    public List<PlayerManager> players = new();
    [SerializeField] LayerMask playerOneUI;
    [SerializeField] LayerMask playerTwoUI;
    [SerializeField] BackUpPlayerSpawner backUpPlayerSpawner;
    public static event Action<int> OnPlayerJoin;
    public static event Action OnPlayerLeave;
    [Space(25)]
    public bool isTesting = false;
    private void Awake()
    {
        Time.timeScale = 1.0f;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //Debug.Log("Setting Game Manager Instance");
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

        if(players.Count == 1)
        {
            player.pCamera.myCamera.cullingMask |= playerOneUI.value;
            player.coloring.SetColors(0);
        }
        else if(players.Count == 2)
        {
            player.pCamera.myCamera.cullingMask |= playerTwoUI.value;
            player.coloring.SetColors(2);
        }

        Debug.Log("Player Added");
        if (!isTesting)
        {
            player.TogglePControl(false);
            player.TogglePCamera(false);
            player.TogglePVisual(false);
        }

        if(trainHandler != null)
        {
            player.transform.SetParent(trainHandler.transform);

            player.MovePlayerTo(trainHandler.playerSpawnPoint[player.GetPlayerIndex()].position);
            if (trainHandler.trainState == TrainState.Parked)
            {
               trainHandler.DisembarkSinglePlayer(player);
                Debug.Log("Not Moving Disembarking");
            }
        }
        else
        {
            player.transform.SetParent(this.transform);
            SpawnPlayer(player);
        }

        OnPlayerJoin?.Invoke(player.GetPlayerIndex());
    }
    private void SpawnPlayer(PlayerManager player)
    {
        if(backUpPlayerSpawner == null)
            return;

        StartCoroutine(backUpPlayerSpawner.SpawnPlayer(player));
       
    }
    public void RemovePlayer(PlayerManager player)
    {
        // remove disconnected players
        OnPlayerLeave?.Invoke();
        players.Remove(player);

        Destroy(player.gameObject);
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
            if(player.pHealth.CurrentLives > 0)
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
