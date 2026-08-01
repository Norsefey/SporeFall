using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliaPickup : DropsPoolBehavior
{
    [SerializeField] private bool despawn = false;
    [SerializeField] private float despawnTime = 5;

    private float amountToGive = 0;
    public void Setup(float dropAmount, bool despawn)
    {
        //Debug.Log($"Mycelia Pickup Setup with amount: {dropAmount}");

        amountToGive = dropAmount;
        this.despawn = despawn;

        if (despawn)
            Invoke(nameof(ReturnObject), despawnTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Stanley")) && despawn)
        {
            if (pool != null)
            {
                PickupMycelia();
            }else
            {
                //Debug.LogWarning("No pool for Mycelia Pick up");
                GameManager.Instance.IncreaseMycelia(amountToGive);
                Destroy(gameObject);
            }
        }
        else
        {
            if (other.CompareTag("Player") || other.CompareTag("Stanley"))
            {
                GameManager.Instance.IncreaseMycelia(amountToGive);
                StartCoroutine(HideObject());
            }
        }
    }

    IEnumerator HideObject()
    {
        GetComponent<Collider>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
       
        yield return new WaitForSeconds(despawnTime);
        
        GetComponent<Collider>().enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void PickupMycelia()
    {
        GameManager.Instance.IncreaseMycelia(amountToGive);
        ReturnObject();
    }
}
