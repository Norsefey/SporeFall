using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public Dictionary<GameObject, ProjectilePool> projectilePool = new();
    public Dictionary<GameObject, VFXPool> vfxPool = new();
    public Dictionary<GameObject, DropsPool> dropsPool = new();

    [Header("Projectiles")]
    [SerializeField] private List<GameObject> projectiles;
    public Transform projectileParent;
    [Header("VFX")]
    [SerializeField] private List<GameObject> VisualEffects;
    public Transform VFXParent;
    [Header("Drops")]
    [SerializeField] private List<GameObject> Drops;
    public Transform dropsParent;

    [SerializeField] private int initialSize;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        foreach (var bullet in projectiles) 
        {
            if (!projectilePool.ContainsKey(bullet))
            {
                projectilePool.Add(bullet, new ProjectilePool(bullet, projectileParent, initialSize));
            }
        }
        foreach (var VFX in VisualEffects)
        {
            if (!vfxPool.ContainsKey(VFX))
            {
                vfxPool.Add(VFX, new VFXPool(VFX, VFXParent, initialSize));

            }
        }
        foreach (var drop in Drops)
        {
            if (!dropsPool.ContainsKey(drop))
            {
                dropsPool.Add(drop, new DropsPool(drop, dropsParent, initialSize));
            }
        }
    }

    public void ReturnALlDrops()
    {

        foreach(Transform drop in dropsParent)
        {
            if (drop.gameObject.activeSelf)
            {
                drop.GetComponent<DropsPoolBehavior>().ReturnObject();
            }
        }

        /*foreach(Transform drop in dropsParent)
        {
            
            
            *//*if (drop.gameObject.activeSelf)
            {
                DropsPoolBehavior dropsPool = drop.GetComponent<DropsPoolBehavior>();
                dropsPool.pool.Return(dropsPool);
            }*//*
                
        }*/
    }
}
