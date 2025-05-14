using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SecretBossSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject bossPrefab;

    public List<GameObject> aegisUnits = new();

    private void Start()
    {
        foreach(GameObject unit in aegisUnits)
        {
            unit.GetComponent<DestructibleObjects>().OnDeath += CheckActiveUnits;
        }
    }
    private void CheckActiveUnits(GameObject unit)
    {
        aegisUnits.Remove(unit);

        if (aegisUnits.Count <= 0)
            SpawnBoss();
    }
    public void SpawnBoss()
    {
        EndlessEnemy boss = Instantiate(bossPrefab, spawnPoint).GetComponent<EndlessEnemy>();

        int index = Random.Range(0, GameManager.Instance.players.Count);
        Transform playerTransform = GameManager.Instance.players[index].pController.transform;

        boss.SetTarget(playerTransform);
        boss.Initialize();

        NavMeshAgent agent = boss.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPoint.position); // This helps ensure proper NavMesh placement

            // If somehow still off NavMesh, try to find closest NavMesh point
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnPoint.position, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }

        boss.TriggerRiseAnimation();
    }
}
