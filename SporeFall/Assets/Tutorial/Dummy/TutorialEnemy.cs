using UnityEngine;

public class TutorialEnemy : Damageable
{
    [Tooltip("Guaranteed weapon drop for next tutorial stage")]
    [SerializeField] private GameObject weaponPickUp;
    [SerializeField] private TrainingRoom_Tutorial tutorial;
    private void Awake()
    {
        currentHP = maxHP;
    }
    public override void TakeDamage(float damage)
    {
        Debug.Log("Base took damage");
        base.TakeDamage(damage);
    }
    protected override void Die()
    {
        // what happens upon death
        if (weaponPickUp != null)
        {
            weaponPickUp.SetActive(true);
        }

        Tutorial.Instance.ProgressTutorial();
        Debug.Log("Dying Now");
        Destroy(transform.parent.gameObject);
    }

}
