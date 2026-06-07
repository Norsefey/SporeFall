using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    [Space(5)]
    [SerializeField] private GameObject controlScriptObject;
    [Space(5)]
    [SerializeField] private StructureStats structureStats;

    [SerializeField] private GameObject[] levelVisuals;
    [SerializeField] private GameObject radiusIndicator;
    [SerializeField] private StructureHP healthComponent;
    [SerializeField] private GameObject deathVFX;
    [Header("LayerMasks")]
    public LayerMask placeableLayer;
    public LayerMask collisionOverlapLayer;

    [HideInInspector]
    public StructurePoolBehavior poolBehavior;
    private IStructureStats structureBehavior;
    public bool onPlatform = false;
    public PlatformStructure myPlatform;
    private void Awake()
    {
        poolBehavior = GetComponent<StructurePoolBehavior>();
        structureBehavior = GetComponent<IStructureStats>();
        UpdateRadiusVisual();
    }
    public void Initialize()
    {
        UpdateVisuals();
        UpdateStats();
        structureBehavior?.Initialize(structureStats.currentLevel);
    }
    public void UpdateRadiusVisual()
    {
        // Update radius indicator
        if (radiusIndicator != null)
        {
            float scale = 0;
            switch (structureStats.currentLevel)
            {
                case TurretLevel turret:
                    scale = turret.range * 2;
                    break;
                case FlameThrowerLevel flamey:
                    scale = flamey.range * 2;
                    break;
                case RepairLevel repairStation:
                    scale = repairStation.healRange;
                    break;
                case LilyLevel lily:
                    scale = lily.patrolRange * 2;
                    break;
                case WallLevel wall:
                    scale = wall.protectionRange * 2;
                    break;
                case MortyLevel morty:
                    scale = morty.detectionRange * 2;
                    break;

            }
            radiusIndicator.transform.localScale = new Vector3(scale, 0.3f, scale);

        }
    }
    public void ShowRadius(bool show)
    {
        if (radiusIndicator != null)
            radiusIndicator.SetActive(show);
    }
    public bool CanUpgrade(float availableMycelia)
    {
        return availableMycelia >= structureStats.currentLevel.GetUpgradeCost();
    }
    public void Upgrade()
    {
        UpdateVisuals();
        UpdateStats();
    }
    private void UpdateVisuals()
    {
        if(structureStats.currentLevel == null || structureStats.currentLevel.level < 0 || structureStats.currentLevel.level >= levelVisuals.Length)
        {
            //Debug.LogError($"Structure {structureStats.structureName} has no current level assigned.");
            return;
        }

        // Deactivate current visual if it exists
        if (structureStats.currentLevel.level - 1 >= 0)
        {
            levelVisuals[structureStats.currentLevel.level - 1].SetActive(false);
        }
        // Get and activate new visual
        if (structureStats.currentLevel.level < levelVisuals.Length && levelVisuals[structureStats.currentLevel.level] != null)
        {
            levelVisuals[structureStats.currentLevel.level].SetActive(true);
        }
    }
    private void UpdateStats()
    {        
        // Set the new HP value
        healthComponent.SetMaxHP(structureStats.currentLevel.maxHealth );
        
        structureBehavior?.UpdateStats(structureStats.currentLevel);
        UpdateRadiusVisual();
    }
    public void ToggleStructureBehavior(bool toggle)
    {
        controlScriptObject.SetActive(toggle);
        // prevent structure from taking damage when not active
        healthComponent.canTakeDamage = toggle;
    }
    public float CalculateStructureRefund(float minimumRefundPercent)
    {
        StructureHP structureHP = GetStructureHP();

        // Calculate HP percentage (clamped between 0 and 1)
        float healthPercentage = Mathf.Clamp01(structureHP.CurrentHP / structureHP.MaxHP);
        // Calculate refund percentage, scaled between minimumRefundPercent and 1
        float refundPercentage = Mathf.Lerp(minimumRefundPercent, 1f, healthPercentage);
        // Calculate final refund amount and round to nearest integer
        int refundAmount = Mathf.RoundToInt(GetCurrentMyceliaCost() * refundPercentage);

        return refundAmount;
    }
    public void ReturnToPool()
    {
        //Debug.Log("Returning To Pool");
        DisableStructureControls();

        SpawnDeathVFX();
        if(onPlatform && myPlatform != null)
        {
            myPlatform.RemoveStructure();
        }else if (controlScriptObject.transform.GetChild(0).TryGetComponent(out PlatformStructure platform))
        {
            Debug.Log("Platform Removing Holding Structure");
            platform.RemoveStructure();
        }

        if (GameManager.Instance != null)
            GameManager.Instance.RemoveStructure(this);

        poolBehavior.ReturnObject();
    }
    public void DisableStructureControls()
    {
        controlScriptObject.SetActive(false);
    }
    private void OnDisable()
    {
        controlScriptObject.SetActive(false);
    }
    private void SpawnDeathVFX()
    {
        VFXPoolingBehavior vfx;
        if (PoolManager.Instance != null)
        {
            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(deathVFX, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {deathVFX.name}");
                return;
            }
            vfx = pool.Get(transform.position, Quaternion.identity);
            vfx.Initialize(pool);
        }
        else
        {
            // No pool spawn and enabled VFX
            vfx = Instantiate(deathVFX, transform.position, Quaternion.identity).GetComponent<VFXPoolingBehavior>();
            vfx.gameObject.SetActive(true);
        }
    }
    // Getter methods
    public float GetCurrentMyceliaCost() => structureStats.currentLevel.GetUpgradeCost();
    public float GetCurrentEnergyCost() => structureStats.currentLevel.energyCost;
    public int GetCurrentLevelInt() => structureStats.currentLevel.level;
    public StructureLevel GetCurrentLevel() => structureStats.currentLevel;
    public StructureHP GetStructureHP() => healthComponent;
    public GameObject GetCurrentVisual() 
    { 
        int index = Mathf.Min(structureStats.currentLevel.level, levelVisuals.Length - 1);
        return levelVisuals[index];
    }
    public string GetStructureName() => structureStats.structureName;
    public string GetStructureDescription() => structureStats.description;
    public StructureType GetStructureType() => structureStats.type;
    public Sprite GetStructureIcon() => structureStats.icon;
    public StructureStats GetStructureStats() => structureStats;
}
