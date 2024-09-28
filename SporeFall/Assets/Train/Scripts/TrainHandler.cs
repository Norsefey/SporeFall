using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHandler : MonoBehaviour
{
    public GameObject trainCamera;
    [SerializeField] private Transform trainVisual;
    [SerializeField] private GameObject payloadPrefab;
    [SerializeField] private Transform payloadSpawnPos;
    [SerializeField] private GameObject cannonButton;
    public enum TrainState
    {
        parked,
        moving
    }

    public TrainState state;

    public void SetParkedState()
    {
        state = TrainState.parked;
        trainVisual.rotation = Quaternion.identity;
        trainCamera.SetActive(false);
    }
    public void SetMovingState()
    {
        state = TrainState.moving;
        trainVisual.rotation = Quaternion.Euler(0,70,0);
        trainCamera.SetActive(true);
    }
    public void SpawnPayload(Vector3 position)
    {
        Payload payload = Instantiate(payloadPrefab, payloadSpawnPos).GetComponent<Payload>();

        payload.SetDestination(position);
        payload.StartMoving();
    }
}
