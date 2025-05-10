using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Structure : MonoBehaviour
{
    [Space(5)]
    [SerializeField] private GameObject controlScriptObject;
    [Space(5)]
    public StructureLevels structureStats;
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
    private int currentLevel = 0;


    public bool onPlatform = false;
    public PlatformStructure myPlatform;
    private void Awake()
    {
        poolBehavior = GetComponent<StructurePoolBehavior>();
        structureBehavior = GetComponent<IStructureStats>();
        UpdateRadiusVisual(structureStats.GetLevel(currentLevel));
    }
    public void Initialize()
    {
        if (structureStats == null || structureStats.GetLevelCount() == 0)
        {
            Debug.LogError($"No levels configured for structure: {gameObject.name}");
            return;
        }

        UpdateVisuals();
        UpdateStats();
        structureBehavior?.Initialize(structureStats, currentLevel);
    }
    public void UpdateRadiusVisual(StructureLevel level)
    {
        // Update radius indicator
        if (radiusIndicator != null)
        {
            float scale = 0;
            switch (level)
            {
                case TurretLevel turret:
                    scale = turret.detectionRange * 2;
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
        if (currentLevel >= structureStats.GetLevelCount() - 1) return false;

        var nextLevel = structureStats.GetLevel(currentLevel + 1);
        return availableMycelia >= nextLevel.cost;
    }
    public void Upgrade()
    {
        if (currentLevel >= structureStats.GetLevelCount() - 1) return;

        currentLevel = GameManager.Instance.upgradeManager.GetStructureLevel(structureStats.type);
        UpdateVisuals();
        UpdateStats();
        structureBehavior?.UpdateStats(structureStats, currentLevel);
    }
    private void UpdateVisuals()
    {
        // Deactivate current visual if it exists
        if(currentLevel - 1 >= 0)
        {
            levelVisuals[currentLevel - 1].SetActive(false);
        }
        // Get and activate new visual
        if (currentLevel < levelVisuals.Count() && levelVisuals[currentLevel] != null)
        {
            levelVisuals[currentLevel].SetActive(true);
        }
    }
    private void UpdateStats()
    {
        var levelData = structureStats.GetLevel(currentLevel);
        // Set the new HP value
        healthComponent.SetMaxHP(levelData.maxHealth);

        UpdateRadiusVisual(levelData);
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
    public float GetCurrentMyceliaCost() => structureStats.GetLevel(currentLevel).cost;
    public float GetCurrentEnergyCost() => structureStats.GetLevel(currentLevel).energyCost;
    public int GetCurrentLevel() => currentLevel;
    public bool IsMaxLevel() => currentLevel >= structureStats.GetLevelCount() - 1;
    public StructureLevels GetLevels() => structureStats;
    public StructureHP GetStructureHP() => healthComponent;
    public GameObject GetCurrentVisual() => levelVisuals[currentLevel];
    public string GetStructureName() => structureStats.GetLevel(currentLevel).name;
    public string GetStructureDescription() => structureStats.description;
    public StructureType GetStructureType() => structureStats.type;

}
