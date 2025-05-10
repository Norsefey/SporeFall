using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliaPickup : DropsPoolBehavior
{
    public float minMyceliaAmount;
    public float maxMyceliaAmount;
    private float amountToGive;

    [SerializeField] private bool despawn = false;
    [SerializeField] private float despawnTime = 5;

    public void Setup()
    {
        amountToGive = Mathf.RoundToInt(Random.Range(minMyceliaAmount, maxMyceliaAmount));
        if(despawn)
            Invoke(nameof(ReturnObject), despawnTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Stanley"))
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
