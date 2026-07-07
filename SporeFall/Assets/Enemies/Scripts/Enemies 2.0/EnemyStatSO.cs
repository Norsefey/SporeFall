using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat")]
public class EnemyStatSO : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Fungal Fucker";

    [Header("Base Stats (Level 1)")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;
    [Tooltip("Percentage Damage Reduction"), Range(0f, 1f)]
    public float baseArmor = 0f;
    public float baseMyceliaDropAmount = 10f;

    [Header("Targeting")]
    [Tooltip("Max entities that can target/attack this enemy simultaneously")]
    public int maxTokens = 5;

    [Tooltip("Priority weight when choosing a target")]
    public TargetPriority targetPriority = TargetPriority.Balanced;

    [Tooltip("Per-Level multiplier applied")]
    [Header("Scaling")]
    public float healthScale = 0.12f;
    public float damageScale = 0.12f;
    public float myceliaScale = 0.12f;
    public float moveSpeedScale = 0;
    public float percentArmorScale = 0;

    [Header("Weapon Drops")]
    public GameObject[] weaponDropPrefab;
    [Range(0f, 100f)]
    public float dropChance = 50f;
}

public enum TargetPriority
{
    Balanced,
    Player,
    Structures,
    Train,
    MostDamaged,
    HighestHP
}
