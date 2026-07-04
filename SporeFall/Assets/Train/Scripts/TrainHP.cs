using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHP : Damageable
{
    public TrainHandler train;
    public float startingHealth = 1000f;

    private void Start()
    {
        targetType = TargetType.None;
        maxHealth = startingHealth;
        _health = maxHealth;
        MakeAlive();
    }
    protected override float OnReceiveDamage(float amount)
    {
        if (!canTakeDamage) return 0f;
        float damageTaken = Mathf.Max(amount - dmgReduction, 0f);
        _health -= damageTaken;
        if (_health <= 0f)
        {
            Die();
        }
        return damageTaken;
    }
    public void SetMaxHP(float maxHP)
    {
        maxHealth = maxHP;
        _health = maxHP;
    }
    protected override void Die()
    {
        // put other death behavior here
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(train.DestroyTrain());
    }
}
