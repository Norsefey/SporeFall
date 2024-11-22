using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<StructurePoolBehavior> pool;
    private int initialSize;

    public StructurePool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;
        pool = new Queue<StructurePoolBehavior>();
        InitializePool();
    }
    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewDrops();
        }
    }
    private void CreateNewDrops()
    {
        GameObject obj = GameObject.Instantiate(prefab, parent);
        StructurePoolBehavior drop = obj.GetComponent<StructurePoolBehavior>();
        obj.SetActive(false);
        pool.Enqueue(drop);
    }
    public StructurePoolBehavior Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            CreateNewDrops();
        }

        StructurePoolBehavior obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }
    public void Return(StructurePoolBehavior obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
