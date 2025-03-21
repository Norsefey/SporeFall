using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliaPickup : DropsPoolBehavior
{
    public float minMyceliaAmount;
    public float maxMyceliaAmount;
    public float amountToGive;

    public void Setup(float myceliaAmount)
    {
        amountToGive = Random.Range(minMyceliaAmount, maxMyceliaAmount);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pool != null)
            {
                RewardPlayer(other.transform.parent.GetComponent<PlayerManager>());
            }
        }
    }

    public void RewardPlayer(PlayerManager player)
    {
        GameManager.Instance.IncreaseMycelia(amountToGive);
        ReturnObject();
    }
}
