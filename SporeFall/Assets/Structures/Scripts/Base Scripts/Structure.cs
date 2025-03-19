using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    [Space(5)]
    [SerializeField] private GameObject controlScriptObject;
    [Space(5)]
    [SerializeField] private GameObject[] levelVisuals;
    [SerializeField] public StructureLevels structureStats;
    [SerializeField] private GameObject radiusIndicator;
    [SerializeField] private StructureHP healthComponent;
    [HideInInspector]
    public StructurePoolBehavior poolBehavior;

    private TrainHandler train;
    private IStructureStats structureBehavior;
    private int currentLevel = 0;

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
        var levelData = structureStats.GetLevel(currentLevel);
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
    }
    public void ToggleStructureBehavior(bool toggle)
    {
        controlScriptObject.SetActive(toggle);
        // prevent structure from taking damage when not active
        healthComponent.canTakeDamage = toggle;
    }
    public void SetTrainHandler(TrainHandler train)
    {
        this.train = train;
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
        if (train != null)
            train.RemoveStructure(this);

        healthComponent.ResetHealth();
        controlScriptObject.SetActive(false);
        poolBehavior.ReturnObject();
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

}
