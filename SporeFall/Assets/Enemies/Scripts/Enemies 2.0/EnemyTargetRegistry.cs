using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetRegistry : MonoBehaviour
{
    public static EnemyTargetRegistry Instance {  get; private set; }

    [Header("Global Target-Type Base Weights")]
    [Range(0f, 10f)] public float playerWeight;
    [Range(0f, 10f)] public float structureWeight;
    [Range(0f, 10f)] public float trainWeight;
    [Range(0f, 10f)] public float mostDamagedWeight;
    [Range(0f, 10f)] public float highestHPWeight;

    [Header("Scoring Factors")]
    [Tooltip("Higher = enemies prefer closer targets.")]
    public float distancePenaltyScale = 0.05f;
    [Tooltip("Bonus score for targets that still have open token slots.")]
    public float tokenAvailableBonus = 2f;

    public  List<Damageable> _players = new();
    public  List<Damageable> _structures = new();
    public  List<Damageable> _trainWalls = new();
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(Damageable damageable)
    {
        ListFor(damageable.targetType).Add(damageable);
        damageable.OnDied += Unregister;
    }
    public void Unregister(Damageable damageable) 
    { 
        ListFor(damageable.targetType).Remove(damageable);
        damageable.OnDied -= Unregister;
    }

    public Damageable FindBestTarget(Vector3 entityPosition,
                                         float detectionRange,
                                         int entityInstanceID,
                                         TargetPriority priority,
                                         Damageable currentTarget)
    {
        Damageable best = null;
        float bestScore = float.MinValue;

        EvaluateList(_players, entityPosition, detectionRange, entityInstanceID, priority, ref best, ref bestScore);
        EvaluateList(_structures, entityPosition, detectionRange, entityInstanceID, priority, ref best, ref bestScore);
        EvaluateList(_trainWalls, entityPosition, detectionRange, entityInstanceID, priority, ref best, ref bestScore);

        // show in debug if something was found and what it is
        //Debug.Log($"{best} {bestScore}");
        
        return best;
    }
    private void EvaluateList(List<Damageable> list,
                                   Vector3 entityPos,
                                   float range,
                                   int instanceID,
                                   TargetPriority priority,
                                   ref Damageable best,
                                   ref float bestScore)
    {
        float sqRange = range * range;

        for (int i = 0; i < list.Count; i++)
        {
            Damageable d = list[i];
            if (d == null || !d.IsAlive || !d.CanAcceptToken) continue;

            float sqDist = (d.transform.position - entityPos).sqrMagnitude;
            if (sqDist > sqRange) continue;

            float score = Score(d, sqDist, instanceID, priority);
            if (score > bestScore) { bestScore = score; best = d; }
        }
    }
    private float Score(Damageable d, float sqDist, int instanceID, TargetPriority priority)
    {
        // Base weight from target type
        float base_ = d.targetType switch
        {
            TargetType.Player => playerWeight,
            TargetType.Structure => structureWeight,
            TargetType.TrainWall => trainWeight,
            _ => 1f
        };

        // Priority bias multiplier
        float bias = PriorityBias(d.targetType, priority);

        // Distance penalty (use sqDist / some constant to avoid sqrt)
        float distPenalty = Mathf.Sqrt(sqDist) * distancePenaltyScale;

        // Token bonus: reward targets that can still accept us
        float tokenBonus = (d.HasToken(instanceID) || d.CanAcceptToken) ? tokenAvailableBonus : 0f;

        return (base_ * bias + tokenBonus) - distPenalty;
    }

    private static float PriorityBias(TargetType type, TargetPriority priority)
    {
        return priority switch
        {
            TargetPriority.Player => type == TargetType.Player ? 2.5f : 1f,
            TargetPriority.Structures => type == TargetType.Structure ? 2.5f : 1f,
            TargetPriority.MostDamaged => type == TargetType.Structure ? 2.5f : 1f,
            TargetPriority.HighestHP => type == TargetType.Structure ? 2.5f : 1f,
            TargetPriority.Train => type == TargetType.TrainWall ? 2.5f : .5f,
            _ => 1f   // Balanced
        };
    }
    private List<Damageable> ListFor(TargetType t)
    {
        switch (t)
        {
            case TargetType.Player:
                return _players;
            case TargetType.Structure:
                return _structures;
            case TargetType.TrainWall:
                return _trainWalls;
            default:
                return null;
        }
    }
}
