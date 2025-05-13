// Interface for objects that can take damage
using System;
using UnityEngine;
// Add this interface to Damageable class

public abstract class Damageable : MonoBehaviour
{
    public bool canHoldCorruption;
    public bool canTakeDamage = true;
    [SerializeField] protected float maxHP;
    protected float currentHP;
    // Variable to store original max health for difficulty scaling
    protected float originalMaxHealth;
    private bool hasStoredOriginalHealth = false;
    public float MaxHP {  get { return maxHP; } }
    public float CurrentHP {  get { return currentHP; } }
    public bool isDead = false;
    protected float damageReduction = 0f;
    // Event that will be triggered when enemy takes damage
    public event Action<Damageable, float> OnHPChange;
    protected abstract void Die();
    public virtual void IncreaseCorruption(float amount)
    {

    }
    private void Start()
    {
        ResetHealth();
        StoreOriginalMaxHealth();
    }
    public virtual void TakeDamage(float damage)
    {
        if(!canTakeDamage)    
        { 
            return; 
        }

        float finalDamage = damage - (damage * damageReduction);

        currentHP -= finalDamage;
        // Trigger the event After damage is taken
        OnHPChange?.Invoke(this, damage);

        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }
    public virtual void ResetHealth()
    {
        isDead = false;
        currentHP = maxHP;

        OnHPChange?.Invoke(this, maxHP);
    }
    public void RestoreHP(float value)
    {
        currentHP = Mathf.Min(currentHP + value, maxHP);

        if(currentHP > maxHP)
            currentHP = maxHP;

        OnHPChange?.Invoke(this, currentHP);

    }
    public void SetMaxHP(float value)
    {
        maxHP = value;
        ResetHealth();
    }

    // Methods for difficulty scaling in endless waves
    public void StoreOriginalMaxHealth()
    {
        if (!hasStoredOriginalHealth)
        {
            originalMaxHealth = maxHP;
            hasStoredOriginalHealth = true;
        }
    }
    public void SetDamageReduction(float newModifier)
    {
        damageReduction = newModifier;
    }
    public bool HasStoredOriginalHealth()
    {
        return hasStoredOriginalHealth;
    }
    public void SetMaxHealthWithMultiplier(float multiplier)
    {
        float newMaxHealth = originalMaxHealth * multiplier;
        SetMaxHP(newMaxHealth);
    }
    public void ResetToOriginalHealth()
    {
        if (hasStoredOriginalHealth)
        {
            SetMaxHP(originalMaxHealth);
        }
    }
}
