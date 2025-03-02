// Interface for objects that can take damage
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
    protected abstract void Die();
    protected abstract void UpdateHPUI();
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
        UpdateHPUI();
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

        UpdateHPUI();
    }
    public void RestoreHP(float value)
    {
        currentHP = Mathf.Min(currentHP + value, maxHP);

        if(currentHP > maxHP)
            currentHP = maxHP;

        UpdateHPUI();
    }
    public void SetMaxHP(float value)
    {
        maxHP = value;
        ResetHealth();
    }
}
