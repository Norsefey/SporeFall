using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactables : DropsPoolBehavior
{
    protected PlayerManager player;
    public abstract void Interact(InputAction.CallbackContext context);
    public abstract void ItemPrompt();
    public abstract void RemovePrompt();
    public void DestroyIntractable()
    {
        if(player != null)
            player.pInput.RemoveInteraction(this);
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            Debug.Log("No Pool Destroying");
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform.parent.GetComponent<PlayerManager>();
            player.pInput.AssignInteraction(this);
            ItemPrompt();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RemoveIntractable();
        }
    }

    public void RemoveIntractable()
    {
        player.pInput.RemoveInteraction(this);
        RemovePrompt();
    }
}
