using UnityEngine;

public abstract class RangedAttack : Attack
{
    [Header("Ranged Attack Settings")]
    [SerializeField] protected bool predictTargetPosition = true;
    [SerializeField] protected float predictionTime = 0.5f;
    [SerializeField] protected Vector3 fireOffset;

    protected Vector3 GetPredictedTargetPosition(Transform target, Vector3 attackOrigin)
    {
        if (!predictTargetPosition || target == null) return target.position;

        if (target.TryGetComponent<Rigidbody>(out var targetRb))
        {
            return target.position + targetRb.velocity * predictionTime;
        }

        return target.position;
    }
}
