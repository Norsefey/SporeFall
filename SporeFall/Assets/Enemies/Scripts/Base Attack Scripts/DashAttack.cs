using UnityEngine;

[CreateAssetMenu(fileName = "NewDashAttack", menuName = "Enemy/Attacks/Dash")]
public class DashAttack : MovementAttack
{
    [Header("Dash")]
    public float dashSpeed = 18f;
    [Tooltip("How close the enemy must get to the target's position to register impact.")]
    public float impactRadius = 0.6f;
    [Tooltip("Force applied to the target's Rigidbody on impact (0 = no knockback).")]
    public float knockbackForce = 8f;
    [Tooltip("Extra damage multiplier if the enemy has built up a minimum dash distance.")]
    public float bonusDamageMultiplierAtFullRange = 1.5f;
    public float minRangeForBonus = 4f;

    private Vector3 _dashDir;
    private float _dashDist;
    private float _travelled;

    public override void BeginMovement(MovementAttackDriver ctx)
    {
        Vector3 toTarget = ctx.Target.transform.position - ctx.Instance.Owner.transform.position;
        toTarget.y = 0f;
        _dashDist = toTarget.magnitude;
        _dashDir = toTarget.normalized;
        _travelled = 0f;
    }

    public override bool TickMovement(MovementAttackDriver ctx, float dt)
    {
        float step = dashSpeed * dt;
        ctx.Instance.Owner.transform.position += _dashDir * step;
        _travelled += step;

        // Impact when close enough or overshot
        float remaining = Vector3.Distance(ctx.Instance.Owner.transform.position,
                                           ctx.Target.transform.position);
        return remaining <= impactRadius || _travelled >= _dashDist + 0.5f;
    }

    public override void OnImpact(MovementAttackDriver ctx)
    {
        float damage = ctx.Instance.ScaledDamage;
        if (_dashDist >= minRangeForBonus)
            damage *= bonusDamageMultiplierAtFullRange;

        ctx.Target.ReceiveDamage(damage);

        // Optional knockback
        if (knockbackForce > 0f && ctx.Target.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 force = _dashDir * knockbackForce;
            force.y = 0f;
            rb.AddForce(force, ForceMode.Impulse);
        }

        if (attackVFXPrefab != null)
            Object.Instantiate(attackVFXPrefab, ctx.Target.transform.position, Quaternion.identity);
        if (attackSFX != null)
            AudioSource.PlayClipAtPoint(attackSFX, ctx.Instance.Owner.transform.position);
    }
}
