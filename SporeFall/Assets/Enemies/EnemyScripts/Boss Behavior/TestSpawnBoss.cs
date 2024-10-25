using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnBoss : MonoBehaviour
{
    [SerializeField]GameObject bossPrefab;
    [SerializeField] Transform payload;
    [SerializeField] Transform player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            BaseBoss boss = Instantiate(bossPrefab).GetComponent<BaseBoss>();

            boss.AssignReferences(payload, player);
        }
    }

}
