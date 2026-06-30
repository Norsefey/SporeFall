
public class StructureHP : Damageable
{
    public Structure structure;

    public bool destroyOnDeath = false;


    private void OnEnable()
    {
        targetType = TargetType.Structure;
        MakeAlive();
        EnemyTargetRegistry.Instance?.Register(this);
    }
    private void OnDisable()
    {
        EnemyTargetRegistry.Instance?.Unregister(this);
    }

    protected override float OnReceiveDamage(float amount)
    {
        _health -= amount;
        if (_health <= 0f) Die();
        return amount;
    }

    // Handle death and destroy the parent object
    protected override void Die()
    {
        base.Die();
        if (destroyOnDeath)
            structure.gameObject.SetActive(false);
        else
            structure.ReturnToPool();
    }
}