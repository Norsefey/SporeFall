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
    public GameUIManager gameUIManager;
    public List<PlayerManager> players = new();
    public PauseMenu pauseMenu;
    public bool paused = false;

    [SerializeField] BackUpPlayerSpawner backUpPlayerSpawner;

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
    public void HandlePlayerJoining(PlayerManager player)
    {
        // keep track of active players
        players.Add(player);
        player.train = trainHandler;

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
    // Called when object is destroyed (including scene changes)
    private void OnDestroy()
    {
        // Clear static instance if this is the current instance
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
