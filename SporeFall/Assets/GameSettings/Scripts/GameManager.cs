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
    // structures
    public Transform structureHolder;
    public List<GameObject> availableStructures;
    private float mycelia = 150;
    public float Mycelia { get { return mycelia; } }
    private bool tutorialMycelia = true;
    public GameObject maxUpgradeButton;

    [Header("Coop Manager")]
    public List<PlayerManager> players = new();
    [SerializeField] LayerMask playerOneUI;
    [SerializeField] LayerMask playerTwoUI;
    [SerializeField] BackUpPlayerSpawner backUpPlayerSpawner;
    public static event Action<int> OnPlayerJoin;
    public static event Action OnPlayerLeave;

    [Header("Difficulty Adjustments")]
    [Space(5)]
    [Header("Normal Difficulty")]
    [SerializeField] private float maxTrainHP_N = 1000;
    [SerializeField] private float trainDamageReduction_N = 0;
    [SerializeField] private float startingMycelia_N = 150;
    [Header("Easy Difficulty")]
    [SerializeField] private float maxTrainHP_E = 2000;
    [SerializeField] private float trainDamageReduction_E = .25f;
    [SerializeField] private float startingMycelia_E = 200;

    private int sceneIndex;

    [Space(25)]
    public bool isTesting = false;
    private void Awake()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex == 1)
        {
            SavedSettings.currentLevel = "GlowingForest";
        }
        else if (sceneIndex == 5)
        {
            SavedSettings.currentLevel = "TrainingLevel";
        }
        else if (sceneIndex == 7)
        {
            SavedSettings.currentLevel = "ToxicSwamp";
        }

        Time.timeScale = 1.0f;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //Debug.Log("Setting Game Manager Instance");
        Instance = this;

        if(PersistentGameManager.Instance != null && trainHandler != null)
        {
            Debug.Log("Persistent manager and train handler are not null");
            if (PersistentGameManager.Instance.GetEasyMode())
            {// Easy Difficulty settings
                Debug.Log("Easy mode");
                if (trainHandler.trainHP != null)
                {
                    Debug.Log("Setting max HP");
                    trainHandler.trainHP.SetMaxHP(maxTrainHP_E);
                    trainHandler.trainHP.damageModifier = trainDamageReduction_E;
                }
                mycelia = startingMycelia_E;
            }
            else // Normal Difficulty settings
            {
                if(trainHandler.trainHP != null)
                {
                    trainHandler.trainHP.SetMaxHP(maxTrainHP_N);
                    trainHandler.trainHP.damageModifier = trainDamageReduction_N;
                }
                mycelia = startingMycelia_N;
            }
        }
    }
    private void Start()
    {
        Debug.Log("Player count is: " + players.Count);
        gameUI.DisplayMycelia(mycelia);

        if(PersistentGameManager.Instance != null)
            PersistentGameManager.Instance.ResetCompletionTimer();
    }
    private void Update()
    {
        if (PersistentGameManager.Instance != null)
            PersistentGameManager.Instance.completionTime += Time.deltaTime;

        if (Tutorial.Instance != null)
        {
            if (SavedSettings.currentLevel == "TrainingLevel" && Tutorial.Instance.tutorialPrompt == 18 && tutorialMycelia == true)
            {
                tutorialMycelia = false;
                IncreaseMycelia(25);
            }
        }
    }
    public void HandlePlayerJoining(PlayerManager player)
    {
        // keep track of active players
        // keep track of active players
        players.Add(player);

        if (players.Count == 1)
        {
            // Player 1 settings
            player.pCamera.myCamera.rect = new Rect(0, 0, 1, 1); // Full screen
            player.pCamera.SetOverlayIndex(playerOneUI.value);
            player.coloring.SetColors(0);
        }
        else if (players.Count == 2)
        {
            // Player 2 settings
            player.pCamera.myCamera.rect = new Rect(0.5f, 0, 0.5f, 1); // Right half
            player.pCamera.SetOverlayIndex(playerTwoUI.value);
            player.coloring.SetColors(2);

            // Update Player 1's viewport when a second player joins
            if (players[0] != null)
            {
                players[0].pCamera.myCamera.rect = new Rect(0, 0, 0.5f, 1); // Left half
                                                                            // Re-adjust the overlay camera for player 1
                players[0].pCamera.SetOverlayCamera();
            }
        }

        // Now set up the overlay camera to match the main camera's viewport
        player.pCamera.SetOverlayCamera();

        Debug.Log("Player Added");
        if (!isTesting)
        {
            player.TogglePControl(false);
            player.TogglePCamera(false);
            player.TogglePVisual(false);
        }

        if(trainHandler != null && !isTesting)
        {
            player.transform.SetParent(trainHandler.transform);
            player.transform.localPosition = new Vector3(0, 5, 10);
            player.TogglePVisual(false);
            player.TogglePControl(false);
            player.MovePlayerTo(trainHandler.playerSpawnPoint[player.GetPlayerIndex()].position);
            if (trainHandler.trainState == TrainState.Parked)
            {
                trainHandler.DisembarkSinglePlayer(player);
                Debug.Log("Not Moving Disembarking");
            }
        }
        else
        {
            //player.transform.SetParent(this.transform);
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
    public void ApplyUpgradeToStructures()
    {
        foreach (Transform structure in structureHolder)
        {
            structure.GetComponent<Structure>().Upgrade();
        }

        trainHandler.UpdateEnergyUsage();
    }
    public void IncreaseMycelia(float amount)
    {
        mycelia += amount;
        gameUI.DisplayMycelia(mycelia);
    }
    public void DecreaseMycelia(float amount)
    {
        mycelia -= amount;
        if(mycelia < 0)
            mycelia = 0;
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

    public void UpdatePlayerSensitivity(int i)
    {
        if(players.Count == 0)
            return;

        if (players[i].playerDevice == "Mouse")
        {
            if (i == 0)
            {
                players[i].pCamera.SetMouseP1();
            }

            else if (i > 0)
            {
                players[i].pCamera.SetMouseP2();
            }
        }

        else if (players[i].playerDevice == "Gamepad")
        {
            if (i == 0)
            {
                players[i].pCamera.SetGamepadP1();
            }

            else if (i > 0)
            {
                players[i].pCamera.SetGamepadP2();
            }
        }
    }
}
