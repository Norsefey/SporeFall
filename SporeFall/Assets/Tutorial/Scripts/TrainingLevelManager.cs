using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrainingLevelManager : MonoBehaviour
{
    [SerializeField] private GameObject worldCamera;

    [SerializeField] private DummyBehavior[] shootingRangeEnemies;
    [SerializeField] private Transform[] srSpawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.OnPlayerJoin += GetPlayerDevice;

        foreach(DummyBehavior dummy in shootingRangeEnemies)
        {
            dummy.OnEnemyDeath += RespawnEnemy;
        }
    }
    private void GetPlayerDevice(int playerIndex)
    {
       
        Invoke(nameof(DisableWorldCamera), 2);
        ;
        // remove listener, since it stays even when changing scenes,
        // which leads to errors as this scripts doesn't exist in other scenes
        GameManager.OnPlayerJoin -= GetPlayerDevice;
    }
    private void DisableWorldCamera()
    {
        worldCamera.SetActive(false);
    }

    private void RespawnEnemy(BaseEnemy enemy)
    {
        StartCoroutine(SpawnEnemy(enemy.gameObject));
    }

    private IEnumerator SpawnEnemy(GameObject enemy)
    {
        enemy.transform.position = srSpawnPoints[Random.Range(0, srSpawnPoints.Length)].position;
        yield return new WaitForSeconds(2);
        enemy.SetActive(true);
    }
}
