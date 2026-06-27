using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHP : Damageable
{
    public TrainHandler train;

    private void OnEnable()
    {
        targetType = TargetType.None;
        _health = maxHealth;
    }
    private void OnDisable()
    {
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
