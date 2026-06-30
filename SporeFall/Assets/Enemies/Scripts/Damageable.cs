using System;
using System.Collections.Generic;
using UnityEngine;
public class Damageable : MonoBehaviour
{
    protected float _health;
    [HideInInspector]public float maxHealth;

    [Header("Token Settings")]
    [Tooltip("Max simultaneous attackers.")]
    public int maxTokens = 4;

    [Header("Target Classification")]
    public TargetType targetType = TargetType.Structure;
    private readonly HashSet<int> _holders = new(); // keyed by enemy instanceID

    public bool IsAlive { get; private set; } = true;
    public int TokensUsed => _holders.Count;
    public bool HasToken(int enemyID) => _holders.Contains(enemyID);
    public bool CanAcceptToken => _holders.Count < maxTokens;
    public event Action<Damageable> OnDied;
    // Event that will be triggered when enemy takes damage
    public event Action<Damageable, float> OnHPChange;

    protected float dmgReduction = 0;
    public bool canTakeDamage = true;
    public bool canHoldCorruption = true;

    public void MakeAlive()
    {
        _health = maxHealth;
        IsAlive = true;
        OnHPChange?.Invoke(this, _health);
    }
    public void RestoreHealth(float amount)
    {
        _health += amount;
        _health = Mathf.Max(_health, maxHealth);

        OnHPChange?.Invoke(this, amount);
    }
    public bool TryAcquireToken(int enemyInstanceID)
    {
        if (!IsAlive || !CanAcceptToken) return false;
        _holders.Add(enemyInstanceID);
        return true;
    }
    public void ReleaseToken(int enemyInstanceID)
            => _holders.Remove(enemyInstanceID);
    public float ReceiveDamage(float amount)
    {
        if (!IsAlive) return 0f;

        float dealt = OnReceiveDamage(amount);
        OnHPChange?.Invoke(this, amount);
        return dealt;
    }
    protected virtual float OnReceiveDamage(float amount) => amount;
    protected virtual void Die()
    {
        if (!IsAlive) return;
        IsAlive = false;
        _holders.Clear();
        OnDied?.Invoke(this);
    }
    public void SetDamageReduction(float newModifier)
    {
        dmgReduction = newModifier;
    }
    public void ToggleDamage()
    {
        canTakeDamage = !canTakeDamage;
    }
    public void ToggleCorruption()
    {
        canHoldCorruption = !canHoldCorruption;
    }
    private void OnDisable() => _holders.Clear();
    public float CurrentHealth => _health;

    /* public bool canHoldCorruption;
     public bool canTakeDamage = true;
     [SerializeField] protected float maxHP;
     private float HPVeriance = 10f; // Random variance for HP to add unpredictability
     protected float currentHP;
     // Variable to store original max health for difficulty scaling
     protected float originalMaxHealth;
     private bool hasStoredOriginalHealth = false;
     public float MaxHP {  get { return maxHP; } }
     public float CurrentHP {  get { return currentHP; } }
     protected float damageReduction = 0f;
     // Event that will be triggered when enemy takes damage
     public event Action<Damageable, float> OnHPChange;



     [Tooltip("Max simultaneous attackers.")]
     public int maxTokens = 5;
     public TargetType targetType;

     private readonly HashSet<int> _holders = new();
     public bool IsDead { get; private set; } = false;
     public int TokensUsed >= _holders.Count;


     protected abstract void Die();
     public virtual void IncreaseCorruption(float amount)
     {

     }
     private void Start()
     {
         ResetHealth();
         StoreOriginalMaxHealth();
     }
     public virtual void TakeDamage(float rawDamage)
     {
         if(!canTakeDamage)    
         { 
             return; 
         }

         float finalDamage = rawDamage - (rawDamage * damageReduction);

         currentHP -= finalDamage;
         // Trigger the event After damage is taken
         OnHPChange?.Invoke(this, rawDamage);

         if (currentHP <= 0 && !IsDead)
         {
             IsDead = true;
             Die();
         }
     }
     public virtual void ResetHealth()
     {
         IsDead = false;
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
         float newMaxHealth = (originalMaxHealth + UnityEngine.Random.Range(-HPVeriance, HPVeriance)) * multiplier;
         SetMaxHP(newMaxHealth);
     }
     public void ResetToOriginalHealth()
     {
         if (hasStoredOriginalHealth)
         {
             SetMaxHP(originalMaxHealth);
         }
     }*/
}

public enum TargetType
{
    Player,
    Structure,
    TrainWall,
    Enemy,
    None
}
