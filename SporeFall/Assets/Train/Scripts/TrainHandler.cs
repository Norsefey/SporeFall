using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHandler : MonoBehaviour
{
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

    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] private Vector3 structureCheckSize = Vector3.one; // Size of the overlap check box

    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        // make sure HP is always the first child of train
        if (transform.GetChild(0).TryGetComponent<TrainHP>(out trainHP))
        {// get and assign train HP
            trainHP.train = this;
            tUI.SetMaxHP(trainHP.MaxHP);
        }
        Debug.Log("Train Is awake");
    }
    public void SetParkedState()
    {
        trainState = TrainState.Parked;
        trainVisual.rotation = Quaternion.identity;
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
        Tutorial.Instance.StartPayloadTutorial();
    }
    private void ToggleStructures(bool toggle)
    {
        structureHolder.gameObject.SetActive(toggle);
        if(toggle)
            CheckStructureObstructions();
    }
    public void ApplyUpgradeToStructures()
    {
        foreach(Transform structure in structureHolder)
        {
            structure.GetComponent<Structure>().Upgrade();
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

            Collider[] structureColliders = structure.GetComponentsInChildren<Collider>();
            bool hasOverlap = false;

            foreach (Collider structureCollider in structureColliders)
            {
                bool wasIsTrigger = structureCollider.isTrigger;
                structureCollider.isTrigger = true;

                Bounds bounds = structureCollider.bounds;

                Collider[] overlaps = Physics.OverlapBox(
                    bounds.center,
                    bounds.extents,
                    structure.transform.rotation,
                    obstructionLayer
                );

                structureCollider.isTrigger = wasIsTrigger;

                foreach (Collider overlap in overlaps)
                {
                    // Ignore collisions with the structure itself
                    if (!overlap.transform.IsChildOf(structure.transform))
                    {
                        hasOverlap = true;
                        break;
                    }
                }

                if (hasOverlap) break;
            }

            if (hasOverlap)
            {
                foreach (PlayerManager player in GameManager.Instance.players)
                {
                    GameManager.Instance.IncreaseMycelia(structure.CalculateStructureRefund(0.5f));
                }
                Debug.Log($"{structure.name} Refunded Due to Overlap");
                structuresToRemove.Add(structure);
            }
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
        structure.transform.SetParent(structureHolder, true);
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
        trainCamera.SetActive(false);
        foreach (var player in GameManager.Instance.players)
        {
            if (player.pHealth.CurrentLives <= 0)
            {
                Debug.Log("respawning Player");
                player.pHealth.IncreaseLife();
                player.StartRespawn();
            }

            player.TogglePControl(true);
            player.TogglePVisual(true);
            player.TogglePCamera(true);
            player.TogglePCorruption(true);

           
        }
        listener.enabled = false;
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

       /* if (Payload != null)
            return Payload.transform;
        else*/
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
