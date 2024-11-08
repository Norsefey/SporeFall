using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<VFXPoolingBehavior> pool;
    private int initialSize;

    public VFXPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;
        pool = new Queue<VFXPoolingBehavior>();
        InitializePool();
    }
    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewVFX();
        }
    }
    private void CreateNewVFX()
    {
        GameObject obj = GameObject.Instantiate(prefab, parent);
        VFXPoolingBehavior effect = obj.GetComponent<VFXPoolingBehavior>();
        obj.SetActive(false);
        pool.Enqueue(effect);
    }
    public VFXPoolingBehavior Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            CreateNewVFX();
        }

        VFXPoolingBehavior obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }
    public void Return(VFXPoolingBehavior obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
