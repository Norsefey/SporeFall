using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHandler : MonoBehaviour
{
    // train camera
    public GameObject trainCamera;
    // train visuals
    [SerializeField] private Transform trainVisual;
    // payload
    [SerializeField] private GameObject payloadPrefab;
    [SerializeField] private Transform payloadSpawnPos;
    public Payload Payload { get; private set; }
    // Interactables
    // structures
    public Transform structureHolder;
    private List<Structure> activeStructures = new List<Structure>();
    public float maxEnergy = 50;
    private float energyUsed = 0;
    // train Variables
    public float cannonFireTime = 2f;
    public float trainMoveSpeed = 5f; // Speed of the smooth movement to wave location
    public float maxHP = 100;
    private float currentHP = 100;
    public enum TrainState
    {
        Parked,
        Firing,
        Moving
    }
    public TrainState state;
    public void SetParkedState()
    {
        state = TrainState.Parked;
        trainVisual.rotation = Quaternion.identity;
        trainCamera.SetActive(false);
        ToggleStructures(true);
    }
    public void SetFiringState()
    {
        state = TrainState.Firing;
        trainCamera.SetActive(true);

    }
    public void SetMovingState()
    {
        state = TrainState.Moving;
        trainVisual.rotation = Quaternion.Euler(0,70,0);
        ToggleStructures(false);
    }
    public void SpawnPayload(Vector3 position)
    {
        Payload = Instantiate(payloadPrefab, payloadSpawnPos).GetComponent<Payload>();

        Payload.SetDestination(position);
        Payload.StartMoving();
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
        activeStructures.Add(structure);
        structure.SetTrainHandler(this);
        UpdateEnergyUsage();
    }
    public void RemoveStructure(Structure structure)
    {
        activeStructures.Remove(structure);
        UpdateEnergyUsage();
    }
    public void UpdateEnergyUsage()
    {
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
}
