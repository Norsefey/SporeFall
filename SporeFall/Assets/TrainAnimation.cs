using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainAnimation : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ParkTrain()
    {
        Debug.Log("Parking Train");


        animator.SetTrigger("Park");
    }
    public void SetMovingTrain()
    {
        animator.SetTrigger("Moving");
        animator.ResetTrigger("Moving");

    }
    public void FireCannon()
    {
        animator.SetTrigger("FireCannon");
    }
    public void OpenUpgradesPanel()
    {
        animator.SetTrigger("Upgrades");
    }
}
