using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public Dictionary<GameObject, ProjectilePool> projectilePool = new();
    public Dictionary<GameObject, VFXPool> vfxPool = new();
    public Dictionary<GameObject, VFXPool> dropsPool = new();


    [Header("Projectiles")]
    [SerializeField] private List<GameObject> projectiles;
    [Header("VFX")]
    [SerializeField] private List<GameObject> VisualEffects;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GameObject projectileParent = new GameObject($"Pool_Projectiles");
        projectileParent.transform.SetParent(transform);
        foreach (var bullet in projectiles) 
        {
            if (!projectilePool.ContainsKey(bullet))
            {
                projectilePool.Add(bullet, new ProjectilePool(bullet, projectileParent.transform, 10));

            }
        }
        GameObject VFXParent = new GameObject($"Pool_VFX");
        VFXParent.transform.SetParent(transform);
        foreach (var VFX in VisualEffects)
        {
            if (!vfxPool.ContainsKey(VFX))
            {
                vfxPool.Add(VFX, new VFXPool(VFX, VFXParent.transform, 10));

            }
        }
        /*GameObject dropsParent = new GameObject($"Pool_Drops");
        dropsParent.transform.SetParent(transform);*/

    }

}
