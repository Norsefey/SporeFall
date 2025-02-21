// Interface for objects that can take damage
using UnityEngine;
// Add this interface to Damageable class

public abstract class Damageable : MonoBehaviour
{
    public float maxHP;
    protected float currentHP;
    public bool canHoldCorruption;
    public bool canTakeDamage = true;
    public float CurrentHP {  get { return currentHP; } }
    public bool isDead = false;
    protected abstract void Die();
    protected abstract void UpdateUI();
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
        UpdateUI();
        if(currentHP <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }
    public virtual void ResetHealth()
    {
        isDead = false;
        currentHP = maxHP;
    }
    public void RestoreHP(float value)
    {
        currentHP = Mathf.Min(currentHP + value, maxHP);

        if(currentHP > maxHP)
            currentHP = maxHP;

        UpdateUI();
    }
    public void SetMaxHP(float value)
    {
        maxHP = value;
        ResetHealth();
    }
}
