using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonButton : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerManager>().AssignButtonAction();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerManager>().RemoveButtonAction();
        }
    }
}
