using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHandler : MonoBehaviour
{
    private List<PlayerManager> players = new List<PlayerManager>();
    [Header("References")]
    [SerializeField] private GameObject trainCamera;
    [SerializeField] private Transform trainVisual;
    [SerializeField] private GameObject payloadPrefab;
    [SerializeField] private Transform payloadSpawnPos;
    public Payload Payload { get; private set; }
    [SerializeField] public Transform[] playerSpawnPoint;
   
    [Header("Structures")]
    // structures
    [SerializeField] private Transform structureHolder;
    private List<Structure> activeStructures = new List<Structure>();
    public float maxEnergy = 50;
    private float energyUsed = 0;
    // train Variables
    [Header("Train Movement")]
    public float cannonFireTime = 2f;
    public float trainMoveSpeed = 5f; // Speed of the smooth movement to wave location
    public enum TrainState
    {
        Parked,
        Firing,
        Moving
    }
    public TrainState state;
    [Header("Train Stats")]
    public float maxHP = 100;
    private float currentHP = 100;
    public void SetParkedState()
    {
        state = TrainState.Parked;
        trainVisual.rotation = Quaternion.identity;
        ToggleStructures(true);
        if(players.Count != 0)
        {
            DisembarkTrain();
        }
    }
    public void SetFiringState()
    {
        state = TrainState.Firing;
        trainCamera.SetActive(true);
        BoardTrain();
    }
    public void SetMovingState()
    {
        state = TrainState.Moving;
        trainVisual.rotation = Quaternion.Euler(0,70,0);
        ToggleStructures(false);
    }
    public void SpawnPayload(Transform[] path)
    {
        Payload = Instantiate(payloadPrefab, payloadSpawnPos).GetComponent<Payload>();
        Payload.StartMoving(path);
    }
    private void ToggleStructures(bool state)
    {
        structureHolder.gameObject.SetActive(state);
    }
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
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
            energyUsed += structure.GetEnergyCost();
        }
    }
    public bool CheckEnergy(float eCost)
    {
        return energyUsed + eCost <= maxEnergy;
    }
    private void BoardTrain()
    {
        if (players.Count == 0)
            return;
        // hide players to move train
        foreach(var player in players)
        {
            // switch to train camera
            player.TogglePControl(false);
            player.TogglePCamera(false);
            player.TogglePVisual(false);
            player.MovePlayerTo(playerSpawnPoint[player.GetPlayerIndex()].position);
            player.transform.SetParent(this.transform);
        }
       
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
        }
    }
    public void AddPlayer(PlayerManager player)
    {
        // keep track of active players
        players.Add(player);
        player.train = this;

        Debug.Log("Player Added");

        if (state == TrainState.Moving)
            BoardTrain();
        else
        {
            player.TogglePControl(false);
            player.TogglePCamera(false);
            player.TogglePVisual(false);
            player.MovePlayerTo(playerSpawnPoint[player.GetPlayerIndex()].position);
            player.transform.SetParent(this.transform);

            Invoke("DisembarkTrain", .5f);
        }
    }
    public void CheckTrainCamera()
    {
        if (trainCamera.activeSelf)
        {
            trainCamera.SetActive(false);
        }
    }
    public void RemovePlayer(PlayerManager player)
    {
        // remove disconnected players
        players.Remove(player);
    }
}
