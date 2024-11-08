using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DropsPool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<DropsPoolBehavior> pool;
    private int initialSize;

    public DropsPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;
        pool = new Queue<DropsPoolBehavior>();
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
        DropsPoolBehavior drop = obj.GetComponent<DropsPoolBehavior>();
        obj.SetActive(false);
        pool.Enqueue(drop);
    }
    public DropsPoolBehavior Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            CreateNewDrops();
        }

        DropsPoolBehavior obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }
    public void Return(DropsPoolBehavior obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
