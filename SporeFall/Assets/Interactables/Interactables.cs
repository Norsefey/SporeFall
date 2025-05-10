using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactables : DropsPoolBehavior
{
    protected PlayerManager player;
    protected bool interactionEnabled = false;
    public abstract void Interact(InputAction.CallbackContext context);
    public abstract void ItemPrompt();
    public abstract void RemovePrompt();
    public void DestroyIntractable()
    {
        if(player != null && interactionEnabled)
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
            interactionEnabled = true;
            //Debug.Log("Prompting Player");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RemoveIntractable();
        }
    }
    private void OnDisable()
    {
        if(interactionEnabled)
            RemoveIntractable();
    }
    public void RemoveIntractable()
    {
        Debug.Log("Removing Interaction" + gameObject.name);
        player.pInput.RemoveInteraction(this);
        RemovePrompt();

        interactionEnabled = false;
    }
}
