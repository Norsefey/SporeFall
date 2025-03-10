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
    public float MaxHP {  get { return maxHP; } }
    public float CurrentHP {  get { return currentHP; } }
    public bool isDead = false;
    // Event that will be triggered when enemy takes damage
    public event Action<Damageable, float> OnHPChange;
    protected abstract void Die();
    public virtual void IncreaseCorruption(float amount)
    {

    }
    private void Start()
    {
        ResetHealth();
    }
    public virtual void TakeDamage(float damage)
    {
        if(!canTakeDamage)    
        { 
            return; 
        }

        currentHP -= damage;
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
}
