// Interface for objects that can take damage
using UnityEngine;
public abstract class Damageable : MonoBehaviour
{
    public float maxHP;
    protected float currentHP;
    public float CurrentHP {  get { return currentHP; } }
    private bool isDead = false;
    protected abstract void Die();
    protected abstract void UpdateUI();

    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        UpdateUI();
        if(currentHP <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }
    public void RestoreHP(float value)
    {
        currentHP += value;

        if(currentHP > maxHP)
            currentHP = maxHP;

        UpdateUI();
    }

}