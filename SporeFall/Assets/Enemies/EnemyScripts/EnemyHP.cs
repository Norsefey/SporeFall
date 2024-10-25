using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private TMP_Text hpDisplay;
    private void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    protected override void Die()
    {
        // call death on enemy
    }

    protected override void UpdateUI()
    {
        if (hpDisplay != null)
        {
            hpDisplay.text = currentHP.ToString() + "/" + maxHP.ToString();
        }
    }
}
