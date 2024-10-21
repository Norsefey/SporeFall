using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHP : MonoBehaviour
{
    public TrainHandler train;
    public float maxHP = 100;
    [SerializeField] private float currentHP = 100;

    public void TakeDamage(float damage)
    {
        Debug.Log(this.name + " Received Damage: " + damage);
        currentHP -= damage;
        train.tUI.UpdateHPDisplay(currentHP);
    }
}
