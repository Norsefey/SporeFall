using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private TMP_Text hpDisplay;
    [SerializeField] private BaseEnemy manager;
    private void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        manager.CheckDamageThreshold(maxHP - currentHP);
        manager.recentDamage.Enqueue(new BaseEnemy.DamageInstance(damage, Time.time));
    }
    protected override void Die()
    {
        // call death on enemy
        manager.Die();
    }
    protected override void UpdateUI()
    {
        if (hpDisplay != null)
        {
            hpDisplay.text = currentHP.ToString("F0") + "/" + maxHP.ToString();
        }
    }
}
