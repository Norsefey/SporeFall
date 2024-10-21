using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliaPickup : MonoBehaviour
{
    public float mycelia;

    public void Setup(float myceliaAmount)
    {
        mycelia = myceliaAmount;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerManager>().mycelia += mycelia;

            Destroy(gameObject);
        }
    }
}
