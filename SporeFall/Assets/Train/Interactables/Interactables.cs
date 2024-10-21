using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactables : MonoBehaviour
{
    protected PlayerManager player;
    public abstract void Interact(InputAction.CallbackContext context);
    public abstract void ItemAction();
    public abstract void RemoveAction();
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name);
            player = other.transform.parent.GetComponent<PlayerManager>();
            player.pInput.AssignInteraction(this);
            ItemAction();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.pInput.RemoveInteraction(this);
            RemoveAction();
            player = null;
        }
    }
}
