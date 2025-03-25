using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHP : Damageable
{
    public TrainHandler train;
    private void Start()
    {
        currentHP = maxHP;
    }
    protected override void Die()
    {
        // put other death behavior here
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(train.DestroyTrain());
    }
}
