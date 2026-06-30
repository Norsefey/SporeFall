using UnityEngine;

public class EnemyStatBlock
{
   public int Level {  get; private set; }
    public float MaxHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float Armor {  get; private set; }
    public float MyceliaDropAmount { get; private set; }

    public void Apply(EnemyStatSO data, int level)
    {
        Level = level;
        
        MaxHealth = data.baseMaxHealth * StatScaler.Multiplier(data.healthScale, level);
        MoveSpeed = data.baseMoveSpeed * StatScaler.Multiplier(data.moveSpeedScale, level);
        Armor = data.baseArmor * StatScaler.Multiplier(data.percentArmorScale, level);
        MyceliaDropAmount = data.baseMyceliaDropAmount * StatScaler.Multiplier(data.myceliaScale, level);

        Debug.Log($"{data.enemyName}- Lv: {level} \n" +
                  $"MaxHealth: {MaxHealth} \n");
    }

    public void Reset()
    {
        Level = 0;
        MaxHealth = 0;
        MoveSpeed = 0;
        Armor = 0;
        MyceliaDropAmount = 0;
    }
}


public static class StatScaler
{
    public static float Multiplier(float ratePerLevel, int level)
        => Mathf.Pow(1f + ratePerLevel, Mathf.Max(0, level - 1));
}