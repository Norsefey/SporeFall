using UnityEngine;

public class TutorialEnemy : Damageable
{
    [Tooltip("Guaranteed weapon drop for next tutorial stage")]
    [SerializeField] private GameObject weaponPickUp;
    [SerializeField] private ShootingRoomTutorial shootingRoomTutorial;

    private void OnEnable()
    {
        targetType = TargetType.Structure;
        _health = maxHealth;
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

    protected override void Die()
    {
        // what happens upon death
        if (weaponPickUp != null)
        {
            weaponPickUp.SetActive(true);
        }

        shootingRoomTutorial.TargetKilled();
        Destroy(transform.parent.gameObject);
    }

}
