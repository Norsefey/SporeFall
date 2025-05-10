using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrainHandler : MonoBehaviour
{
    [Header("References")]
    public WaveManager waveManager;
    public TrainUI UI;
    [SerializeField] private GameObject trainCamera;
    [SerializeField] private Transform trainVisual;
    [SerializeField] private GameObject forceField;
    [SerializeField] private GameObject payloadPrefab;
    [SerializeField] private Transform payloadSpawnPos;
    [SerializeField] private AudioListener listener;
    public TrainAnimation animations;

    public Transform dropsHolder;
    public Payload Payload { get; private set; }
    public Transform[] playerSpawnPoint;
    
    // train Variables
    [Header("Train Movement")]
    public float cannonFireTime = 2f;
    public float trainMoveSpeed = 5f; // Speed of the smooth movement to wave location

    [Header("Train Audio")]
    [SerializeField] private AudioClip movingAudio;
    [SerializeField] private AudioClip fireCannonAudio;
    private AudioSource audioPlayer;
    public enum TrainState
    {
        Parked,
        Firing,
        Moving
    }
    public TrainState trainState;
    [Header("Train Stats")]
    public Transform[] damagePoint;
    public TrainHP trainHP;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] private Vector3 structureCheckSize = Vector3.one; // Size of the overlap check box
    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
    }
    private void Start()
    {
        if (transform.GetChild(0).TryGetComponent<TrainHP>(out trainHP))
        {// get and assign train HP
            trainHP.train = this;
            UI.SetMaxHP(trainHP.MaxHP);
        }
    }
    public void SetParkedState()
    {
        trainState = TrainState.Parked;
        if(GameManager.Instance.players.Count != 0)
        {
            DisembarkTrain();
        }
        audioPlayer.Stop();
    }
    public void SetFiringState()
    {
        trainState = TrainState.Firing;
        animations.FireCannon();
        GameManager.Instance.ReturnAllStructures();

        trainCamera.SetActive(true);
        ClearDrops();
        BoardTrain();

        Invoke(nameof(PlayCannonFireAudio), cannonFireTime - 1);
    }
    public void PlayCannonFireAudio()
    {
        audioPlayer.PlayOneShot(fireCannonAudio);
    }
    public void SetMovingState()
    {
        trainState = TrainState.Moving;
        animations.SetMovingTrain();

        audioPlayer.clip = movingAudio;
        audioPlayer.Play();
    }
    public void SpawnPayload(Transform[] path)
    {
        Payload = Instantiate(payloadPrefab, payloadSpawnPos).GetComponentInChildren<Payload>();
        UI.gameObject.SetActive(false);
        trainHP.canTakeDamage = false;
        Payload.StartMoving(path);
        if(Tutorial.Instance != null) //&& SavedSettings.firstPayloadTutorial
            //SavedSettings.firstPayloadTutorial = false;
            Tutorial.Instance.StartPayloadTutorial();
    }
    public void ToggleForceField(bool toggle)
    {
        forceField.SetActive(toggle);
    }
    public void BoardTrain()
    {
        if (GameManager.Instance.players.Count == 0)
            return;
        // hide players to move train
        foreach (var player in GameManager.Instance.players)
        {
            // switch to train camera
            player.TogglePControl(false);
            player.TogglePCamera(false);
            player.TogglePVisual(false);
            player.TogglePCorruption(false);
            player.MovePlayerTo(playerSpawnPoint[player.GetPlayerIndex()].position);
        }
        listener.enabled = true;
    }
    public void DisembarkTrain()
    {
        // show players once train has parked
        foreach (var player in GameManager.Instance.players)
        {
            // correct player position
            player.StartRespawn(1, false);

            if (player.pHealth.CurrentLives <= 0)
            {
                Debug.Log("Respawning Dead Player");
                player.pHealth.IncreaseLife();
                player.StartRespawn(.5f, true);
            }
            StartCoroutine(DelayCameraToggle(player));
        }
    }
    public void DisembarkSinglePlayer(PlayerManager player)
    {
        // correct player position
        player.StartRespawn(0, false);

        if (player.pHealth.CurrentLives <= 0)
        {
            player.pHealth.IncreaseLife();
            player.StartRespawn(.5f, true);
        }
        StartCoroutine(DelayCameraToggle(player));
    }
    private IEnumerator DelayCameraToggle(PlayerManager player)
    {
        yield return new WaitForSeconds(1);
        listener.enabled = false;
        player.TogglePCamera(true);
        trainCamera.SetActive(false);
    }
    public IEnumerator DestroyTrain()
    {
        // Load Lose Scene
        Debug.Log("Train Destroyed");
        BoardTrain();
        trainCamera.SetActive(true);
        BlowUpTrain();

        yield return new WaitForSeconds(2);

        SceneTransitioner.Instance.LoadLoseScene();
    }
    private void BlowUpTrain()
    {
        GameManager.Instance.waveManager.SpawnExplosion(transform.position);
    }
    public void CheckTrainCamera()
    {
        if (trainCamera.activeSelf)
        {
            trainCamera.SetActive(false);
        }
    }
    public Transform GetDamagePoint()
    {
        int index = Random.Range(0, damagePoint.Length);
        return damagePoint[index];
    }
    private void ClearDrops()
    {
        PoolManager.Instance.ReturnALlDrops();
    }
}
