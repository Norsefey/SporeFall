using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractable : Interactables
{
    public SkinnedMeshRenderer doorMesh; // SkinnedMeshRenderer with the blend shape
    public BoxCollider doorCollider;
    public int blendShapeIndex = 0; // Blend shape index
    public float openDuration = 2f; // Time to fully open the door

    private bool canOpen = true;
    private bool prompted = false;
    public override void Interact(InputAction.CallbackContext context)
    {
        if(canOpen)
            StartCoroutine(OpenDoor());
    }
    public override void ItemPrompt()
    {
        if(canOpen)
            player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} to Open Door");
        else
            player.pUI.EnablePrompt($" <color=red>Door Is Locked</color>");
        prompted = true;
    }
    public override void RemovePrompt()
    {
        player.pUI.DisablePrompt();
        // if door is open close it when player leaves
        if(doorMesh.GetBlendShapeWeight(blendShapeIndex) > 0)
            StartCoroutine(CloseDoor());
        prompted = false;
    }
    private IEnumerator OpenDoor()
    {
        float elapsedTime = 0f;
        while (elapsedTime < openDuration)
        {
            float blendWeight = Mathf.Lerp(0, 100, elapsedTime / openDuration);
            doorMesh.SetBlendShapeWeight(blendShapeIndex, blendWeight);
            
            if(doorMesh.GetBlendShapeWeight(blendShapeIndex) > .45f)
                doorCollider.enabled = false;
           
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches exactly 100 at the end
        doorMesh.SetBlendShapeWeight(blendShapeIndex, 100);
        // Ensure collider has been disabled
        doorCollider.enabled = false;
    }
    private IEnumerator CloseDoor()
    {
        float elapsedTime = 0f;
        while (elapsedTime < openDuration)
        {
            float blendWeight = Mathf.Lerp(doorMesh.GetBlendShapeWeight(blendShapeIndex), 0, elapsedTime / openDuration);
            doorMesh.SetBlendShapeWeight(blendShapeIndex, blendWeight);

            if (doorMesh.GetBlendShapeWeight(blendShapeIndex) < .45f)
                doorCollider.enabled = true;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches exactly 100 at the end
        doorMesh.SetBlendShapeWeight(blendShapeIndex, 0);
        // Ensure collider has been disabled
        doorCollider.enabled = true;
    }

    public void UnlockDoor()
    {
        canOpen = true;
        if(prompted)
            player.pUI.EnablePrompt($"Press {player.pInput.GetInteractionKey()} to Open Door");


    }
}
