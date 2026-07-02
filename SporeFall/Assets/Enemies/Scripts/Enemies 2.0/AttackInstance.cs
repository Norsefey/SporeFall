using UnityEngine;

public class AttackInstance
{
    public Attack Data {  get; private set; }
    public EnemyController Owner { get; private set; }
    public float ScaledDamage { get; private set; }
    public float ScaleCorruption { get; private set; }
    public float AttackRange { get; private set; }

    public float LastUseTime {  get; private set; }
    public float Cooldown { get; private set; }
    public float SelectionWeight => Data.selectionWeight;
    public float MinSelectRange => Data.minSelectRange;

    public void Initialize(Attack data, EnemyController owner, int level, float damageScaleRate)
    {
        Data = data;
        Owner = owner;
        ScaledDamage = Data.baseDamage * StatScaler.Multiplier(damageScaleRate, level);
        ScaleCorruption = Data.baseCorruption * StatScaler.Multiplier(damageScaleRate, level);
        AttackRange = Data.attackRange;
        Cooldown = data.baseCooldown;
        LastUseTime = 0;
    }

    public void Reset()
    {
        Data = null;
        Owner = null;
        ScaledDamage = 0;
        ScaleCorruption = 0;
        AttackRange = 0;
        Cooldown = 0;
        LastUseTime = 0;
    }

    public void SetLastUseTime(float lastUseTime)
    {
        LastUseTime = lastUseTime;
    }
    public bool CanUse()
    {
        if(LastUseTime <= 0) return true;
        if(Time.time > LastUseTime + Cooldown) return true;

        return false;
    }


    public void Execute(Damageable target)
    {
        Data.Execute(this, target);
    }
}
