using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<EnemyController> pool;
    private int initialSize;

    public EnemyObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;
        pool = new Queue<EnemyController>();
        InitializePool();
    }
    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewEnemy();
        }
    }
    private void CreateNewEnemy()
    {
        GameObject obj = GameObject.Instantiate(prefab, parent);
        EnemyController enemy = obj.GetComponent<EnemyController>();
        enemy.name = prefab.name + "_" + pool.Count;
        obj.SetActive(false);
        pool.Enqueue(enemy);
    }
    public EnemyController Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            CreateNewEnemy();
        }

        EnemyController enemy = pool.Dequeue();
        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);
        return enemy;
    }
    public void Return(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }
}
