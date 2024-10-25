using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHP : Damageable
{
    public TrainHandler train;
    private void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }
    protected override void Die()
    {
        // put other death behavior here
        train.DestroyTrain();
    }

    protected override void UpdateUI()
    {
        train.tUI.UpdateHPDisplay(currentHP);
    }
}
