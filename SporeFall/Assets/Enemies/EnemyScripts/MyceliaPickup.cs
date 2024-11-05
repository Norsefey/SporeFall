using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliaPickup : MonoBehaviour
{
    public float myceliaAmount;

    public void Setup(float myceliaAmount)
    {
        this.myceliaAmount = myceliaAmount;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerManager>().mycelia += myceliaAmount;

            Destroy(gameObject);
        }
    }
}
