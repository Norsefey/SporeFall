using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackUpPlayerSpawner : MonoBehaviour
{
    public Transform[] playerSpawnPoints;
    public GameObject tempCamera;

    public IEnumerator SpawnPlayer(PlayerManager player)
    {
        player.MovePlayerTo(playerSpawnPoints[player.GetPlayerIndex()].position);

        yield return new WaitForSeconds(1);
        if(tempCamera.activeSelf)
            tempCamera.SetActive(false);

        player.TogglePControl(true);
        player.TogglePVisual(true);
        player.TogglePCamera(true);
        player.TogglePCorruption(true);
    }
}
