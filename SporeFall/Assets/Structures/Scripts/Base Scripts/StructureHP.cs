using UnityEngine;
using TMPro;

public class StructureHP : Damageable
{
    [SerializeField] private TMP_Text healthDisplay;
    [SerializeField] private Structure structure;
    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }

    // Handle death and destroy the parent object
    protected override void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        structure.ReturnToPool();
    }

    protected override void UpdateHPUI()
    {
        if (healthDisplay != null)
        {
            healthDisplay.text = currentHP.ToString("F0") + "/" + maxHP;
        }
    }
}