using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrainingLevelManager : MonoBehaviour
{
    [SerializeField] private GameObject worldCamera;
    [SerializeField] private MyceliaPickup myceliaToPickUp;

    // Start is called before the first frame update
    void Start()
    {
        SavedSettings.currentLevel = "Training";
        GameManager.OnPlayerJoin += GetPlayerDevice;
        myceliaToPickUp.Setup(500, false);
    }
    private void GetPlayerDevice(int playerIndex)
    {
       
        Invoke(nameof(DisableWorldCamera), 1.5f);
        // remove listener, since it stays even when changing scenes,
        // which leads to errors as this scripts doesn't exist in other scenes
        GameManager.OnPlayerJoin -= GetPlayerDevice;
    }
    private void DisableWorldCamera()
    {
        worldCamera.SetActive(false);
    }
}
