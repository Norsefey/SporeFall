using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHP : Damageable
{
    public TrainHandler train;
    private void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }

    private void Update()
    {
        /*//Testing
        if (Input.GetKeyUp(KeyCode.P))
        {
            currentHP = currentHP - 40;
            UpdateUI();
            Debug.Log("taking 40 damage");
        }*/
    }
    protected override void Die()
    {
        // put other death behavior here
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(train.DestroyTrain());
    }

    protected override void UpdateHPUI()
    {
        train.tUI.UpdateHPDisplay(currentHP);
    }
}
