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
    [SerializeField] private List<GameObject> visualEffects;
    public Transform VFXParent;
    [Header("Weapon Drops")]
    [SerializeField] private List<GameObject> weaponDrops;
    public Transform weaponDropsParent;
    [Header("Mycelia Drops")]
    [SerializeField] private List<GameObject> myceliaDrops;
    public Transform myceliaDropsParent;

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
        foreach (var VFX in visualEffects)
        {
            if (!vfxPool.ContainsKey(VFX))
            {
                vfxPool.Add(VFX, new VFXPool(VFX, VFXParent, initialSize));

            }
        }
        foreach (var weapon in weaponDrops)
        {
            if (!dropsPool.ContainsKey(weapon))
            {
                dropsPool.Add(weapon, new DropsPool(weapon, weaponDropsParent, initialSize));
            }
        }

        foreach (var mycelia in myceliaDrops)
        {
            if (!dropsPool.ContainsKey(mycelia))
            {
                dropsPool.Add(mycelia, new DropsPool(mycelia, myceliaDropsParent, initialSize));
            }
        }
    }

    public void ReturnALlDrops()
    {

        foreach(Transform weapon in weaponDropsParent)
        {
            if (weapon.gameObject.activeSelf)
            {
                weapon.GetComponent<DropsPoolBehavior>().ReturnObject();
            }
        }

        foreach(Transform mycelia in myceliaDropsParent)
        {
            if (mycelia.gameObject.activeSelf)
            {
                GameManager.Instance.TrainHandler.GivePlayersMycelia(mycelia.GetComponent<MyceliaPickup>().myceliaAmount);
                mycelia.GetComponent<DropsPoolBehavior>().ReturnObject();
            }
        }
    }
}