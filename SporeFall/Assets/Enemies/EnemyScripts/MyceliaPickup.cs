using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliaPickup : DropsPoolBehavior
{
    public float minMyceliaAmount;
    public float maxMyceliaAmount;
    private float amountToGive;

    public void Setup()
    {
        amountToGive = Mathf.RoundToInt(Random.Range(minMyceliaAmount, maxMyceliaAmount));
       /* Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);*/
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pool != null)
            {
                PickupMycelia();
            }else
            {
                Debug.LogWarning("No pool for Mycelia Pick up");
                GameManager.Instance.IncreaseMycelia(amountToGive);
                Destroy(gameObject);
            }
        }
    }

    public void PickupMycelia()
    {
        GameManager.Instance.IncreaseMycelia(amountToGive);
        ReturnObject();
    }
}
