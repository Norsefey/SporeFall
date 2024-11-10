using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainHandler : MonoBehaviour
{
    public List<PlayerManager> players = new();
    [Header("References")]
    public WaveManager waveManager;
    [SerializeField] private TrainUI UI;
    [SerializeField] private GameObject trainCamera;
    [SerializeField] private Transform trainVisual;
    [SerializeField] private GameObject forceField;
    [SerializeField] private GameObject payloadPrefab;
    [SerializeField] private Transform payloadSpawnPos;
    [SerializeField] private AudioListener listener;
    public Transform dropsHolder;
    public Payload Payload { get; private set; }
    public Transform[] playerSpawnPoint;
    [Header("Structures")]
    // structures
    [SerializeField] private Transform structureHolder;
    private List<Structure> activeStructures = new();
    public float maxEnergy = 50;
    private float energyUsed = 0;
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

    public TrainUI tUI;
    public TrainHP trainHP;

    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        // make sure HP is always the first child of train
        if (transform.GetChild(0).TryGetComponent<TrainHP>(out trainHP))
        {// get and assign train HP
            trainHP.train = this;
            tUI.SetMaxHP(trainHP.maxHP);
        }
        Debug.Log("Train Is awake");
        trainState = TrainState.Moving;
    }
    public void SetParkedState()
    {
        trainState = TrainState.Parked;
        trainVisual.rotation = Quaternion.identity;
        ToggleStructures(true);
        if(players.Count != 0)
        {
            DisembarkTrain();
        }
        audioPlayer.Stop();
    }
    public void SetFiringState()
    {
        trainState = TrainState.Firing;
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
        trainVisual.rotation = Quaternion.Euler(0,-70,0);
        ToggleStructures(false);

        audioPlayer.clip = movingAudio;
        audioPlayer.Play();
    }
    public void SpawnPayload(Transform[] path)
    {
        Payload = Instantiate(payloadPrefab, payloadSpawnPos).GetComponentInChildren<Payload>();
        UI.gameObject.SetActive(false);
        Payload.StartMoving(path);
    }
    private void ToggleStructures(bool toggle)
    {
        structureHolder.gameObject.SetActive(toggle);
        forceField.SetActive(toggle);
    }
    public void AddStructure(Structure structure)
    {
        // track active structures
        activeStructures.Add(structure);
        // give the structure a reference to the train, so it can remove itself on when destroyed
        structure.SetTrainHandler(this);
        // set the parent of the structure to the structure holder, to hide structures when moving
        structure.transform.SetParent(structureHolder, true);
        // add energy cost of structure to energy usage
        UpdateEnergyUsage();
    }
    public void RemoveStructure(Structure structure)
    {
        activeStructures.Remove(structure);
        UpdateEnergyUsage();
        structure.gameObject.SetActive(false);
        Destroy(structure.gameObject);
    }
    public void UpdateEnergyUsage()
    {
        // since structures can be upgrades and that changes energy usage check all for their current usag
        energyUsed = 0;
        foreach (var structure in activeStructures)
        {
            energyUsed += structure.GetCurrentEnergyCost();
        }
    }
    public bool CheckEnergy(float eCost)
    {
        return energyUsed + eCost <= maxEnergy;
    }
    public void BoardTrain()
    {
        if (players.Count == 0)
            return;
        // hide players to move train
        foreach (var player in players)
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
    private void DisembarkTrain()
    {
        // show players once train has parked
        trainCamera.SetActive(false);
        foreach (var player in players)
        {
            player.TogglePControl(true);
            player.TogglePVisual(true);
            player.TogglePCamera(true);
            player.TogglePCorruption(true);
        }
        listener.enabled = false;
    }
    public void AddPlayer(PlayerManager player)
    {
        // keep track of active players
        players.Add(player);
        player.train = this;

        Debug.Log("Player Added");

        player.TogglePControl(false);
        player.TogglePCamera(false);
        player.TogglePVisual(false);
        player.MovePlayerTo(playerSpawnPoint[player.GetPlayerIndex()].position);
        player.transform.SetParent(this.transform);
        if(trainState == TrainState.Parked)
        {
            Invoke(nameof(DisembarkTrain), .5f);
            Debug.Log("Not Moving Disembarking");
        }
    }
    public void RemovePlayer(PlayerManager player)
    {
        // remove disconnected players
        players.Remove(player);
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
        GameManager.Instance.WaveManager.SpawnExplosion(transform.position);
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
    public void GivePlayersMycelia(float amount)
    {
        foreach(PlayerManager player in players)
        {
            player.IncreaseMycelia(amount);
        }
    }
    private void ClearDrops()
    {
        PoolManager.Instance.ReturnALlDrops();
    }
}
