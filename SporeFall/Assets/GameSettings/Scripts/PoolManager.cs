using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public Dictionary<GameObject, ProjectilePool> projectilePool = new();
    public Dictionary<GameObject, VFXPool> vfxPool = new();
    public Dictionary<GameObject, DropsPool> dropsPool = new();
    public Dictionary<GameObject, StructurePool> structurePool = new();

    [Header("Projectiles")]
    [SerializeField] private int projectileInitialSize;
    [SerializeField] private List<GameObject> projectiles;
    public Transform projectileParent;
    [Header("VFX")]
    [SerializeField] private int VFXInitialSize;
    [SerializeField] private List<GameObject> visualEffects;
    public Transform VFXParent;
    [Header("Weapon Drops")]
    [SerializeField] private int weaponInitialSize;
    [SerializeField] private List<GameObject> weaponDrops;
    public Transform weaponDropsParent;
    [Header("Mycelia Drops")]
    [SerializeField] private int myceliaInitialSize;
    [SerializeField] private List<GameObject> myceliaDrops;
    public Transform myceliaDropsParent;
    [Header("Structures")]
    [SerializeField] private int structureInitialSize;
    //[SerializeField] private List<GameObject> structures;
    public Transform structuresParent;


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
                projectilePool.Add(bullet, new ProjectilePool(bullet, projectileParent, projectileInitialSize));
            }
        }
        foreach (var VFX in visualEffects)
        {
            if (!vfxPool.ContainsKey(VFX))
            {
                vfxPool.Add(VFX, new VFXPool(VFX, VFXParent, VFXInitialSize));

            }
        }
        foreach (var weapon in weaponDrops)
        {
            if (!dropsPool.ContainsKey(weapon))
            {
                dropsPool.Add(weapon, new DropsPool(weapon, weaponDropsParent, weaponInitialSize));
            }
        }

        foreach (var mycelia in myceliaDrops)
        {
            if (!dropsPool.ContainsKey(mycelia))
            {
                dropsPool.Add(mycelia, new DropsPool(mycelia, myceliaDropsParent, myceliaInitialSize));
            }
        }
        foreach (var structure in GameManager.Instance.availableStructures)
        {
            if (!structurePool.ContainsKey(structure))
            {
                structurePool.Add(structure, new StructurePool(structure, structuresParent, structureInitialSize));
            }
        }
    }

    public void ReturnALlDrops()
    {

        foreach(Transform weapon in weaponDropsParent)
        {
            if (weapon.gameObject.activeSelf)
            {
                weapon.GetComponent<PickUpWeapon>().DestroyIntractable();
            }
        }

        foreach(Transform mycelia in myceliaDropsParent)
        {
            if (mycelia.gameObject.activeSelf)
            {
                mycelia.GetComponent<MyceliaPickup>().PickupMycelia();
            }
        }
    }
}
