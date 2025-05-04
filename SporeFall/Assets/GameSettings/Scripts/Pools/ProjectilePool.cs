using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<BaseProjectile> pool;
    private int initialSize;

    public ProjectilePool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;
        pool = new Queue<BaseProjectile>();
        InitializePool();
    }
    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewProjectile();
        }
    }

    private void CreateNewProjectile()
    {
        GameObject obj = GameObject.Instantiate(prefab, parent);
        BaseProjectile projectile = obj.GetComponent<BaseProjectile>();
        obj.SetActive(false);
        pool.Enqueue(projectile);
    }

    public BaseProjectile Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            CreateNewProjectile();
        }

        BaseProjectile projectile = pool.Dequeue();
        projectile.transform.position = position;
        projectile.transform.rotation = rotation;
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public void Return(BaseProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
    }

}
