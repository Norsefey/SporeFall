using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<BaseEnemy> pool;
    private int initialSize;

    public EnemyObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;
        pool = new Queue<BaseEnemy>();
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
        BaseEnemy enemy = obj.GetComponent<BaseEnemy>();
        obj.SetActive(false);
        pool.Enqueue(enemy);
    }
    public BaseEnemy Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            CreateNewEnemy();
        }

        BaseEnemy enemy = pool.Dequeue();
        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);
        enemy.Initialize();
        return enemy;
    }
    public void Return(BaseEnemy enemy)
    {
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }
}
