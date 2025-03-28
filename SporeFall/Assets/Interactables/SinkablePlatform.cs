using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SinkablePlatform : Interactables
{
    public float sinkDelay = 2f; // Time before it starts sinking
    public float sinkSpeed = 1f; // Speed of sinking
    public float sinkDistance = 2f; // How far it sinks
    public float riseDelay = 2f; // Time before it rises back up
    public float riseSpeed = 1f; // Speed of rising

    private Vector3 originalPosition;
    private bool isSinking = false;
    private bool isRising = false;
    private float timer = 0f;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    public override void Interact(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public override void ItemPrompt()
    {
        Debug.Log("Player Standing On lilypad");
        StartCoroutine(SinkAndRise());
    }

    public override void RemovePrompt()
    {
        //throw new System.NotImplementedException();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(!isSinking || !isRising)
            {
                StartCoroutine(SinkAndRise());
            }
        }
    }
    private IEnumerator SinkAndRise()
    {
        if (isSinking || isRising) yield break; // Prevent multiple triggers

        isSinking = true;
        yield return new WaitForSeconds(sinkDelay);

        // Move down
        Vector3 targetPosition = originalPosition - new Vector3(0, sinkDistance, 0);
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, sinkSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(riseDelay); // Wait before rising

        isSinking = false;
        isRising = true;

        // Move up
        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, riseSpeed * Time.deltaTime);
            yield return null;
        }

        isRising = false;
    }
}
