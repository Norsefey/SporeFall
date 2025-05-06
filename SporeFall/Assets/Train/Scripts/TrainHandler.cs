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
    [Header("Structures")]

    private List<Structure> activeStructures = new();
    public float maxEnergy = 50;
    private float energyUsed = 0;
    private float energyRemaining = 0;
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
        // make sure HP is always the first child of train
        
        Debug.Log("Train Is awake");
        UI.DisplayEnergy(maxEnergy);
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
        ToggleStructures(true);
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
        ToggleStructures(false);

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
        if(Tutorial.Instance != null && SavedSettings.firstPayloadTutorial)
            SavedSettings.firstPayloadTutorial = false;
            Tutorial.Instance.StartPayloadTutorial();
    }
    private void ToggleStructures(bool toggle)
    {
        GameManager.Instance.structureHolder.gameObject.SetActive(toggle);
        if (!toggle)
        {
            CheckStructureObstructions();
        }
    }
    public void ToggleForceField(bool toggle)
    {
        forceField.SetActive(toggle);
    }
    private void CheckStructureObstructions()
    {
        List<Structure> structuresToRemove = new();
        foreach (Structure structure in activeStructures)
        {
            Debug.Log("Checking Obstructions");

            GameManager.Instance.IncreaseMycelia(structure.CalculateStructureRefund(0.5f));
            Debug.Log($"{structure.name} Refunded Due to No Ground Contact");
            structuresToRemove.Add(structure);
            continue;
        }
        // Remove all the marked structures
        for (int i = 0; i < structuresToRemove.Count; i++)
        {
            RemoveStructure(structuresToRemove[i]);
            structuresToRemove[i].ReturnToPool();
        }
        structuresToRemove.Clear();
    }
    public void AddStructure(Structure structure)
    {
        // track active structures
        activeStructures.Add(structure);
        // give the structure a reference to the train, so it can remove itself on when destroyed
        structure.SetTrainHandler(this);
        // set the parent of the structure to the structure holder, to hide structures when moving
        structure.transform.SetParent(GameManager.Instance.structureHolder, true);
        // add energy cost of structure to energy usage
        UpdateEnergyUsage();
    }
    public void RemoveStructure(Structure structure)
    {
        activeStructures.Remove(structure);
        UpdateEnergyUsage();
    }
    public void UpdateEnergyUsage()
    {
        // since structures can be upgrades and that changes energy usage check all for their current usage
        energyUsed = 0;

        List<Structure> structuresToRemove = new();

        foreach (var structure in activeStructures)
        {
            energyUsed += structure.GetCurrentEnergyCost();

            if(energyUsed > maxEnergy)
            {
                GivePlayersMycelia(structure.CalculateStructureRefund(0.5f));
                structuresToRemove.Add(structure);
            }
        }
        // Remove all the marked structures
        for (int i = 0; i < structuresToRemove.Count; i++)
        {
            Debug.Log("Structure Removed Due to Energy Limit");
            RemoveStructure(structuresToRemove[i]);
            structuresToRemove[i].ReturnToPool();
        }
        structuresToRemove.Clear();


        energyRemaining = maxEnergy - energyUsed;
        UI.DisplayEnergy(energyRemaining);
    }
    public bool CheckEnergy(float eCost)
    {
        return energyUsed + eCost <= maxEnergy;
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
    public void GivePlayersMycelia(float amount)
    {
        GameManager.Instance.IncreaseMycelia(amount);
    }
    private void ClearDrops()
    {
        PoolManager.Instance.ReturnALlDrops();
    }
}
