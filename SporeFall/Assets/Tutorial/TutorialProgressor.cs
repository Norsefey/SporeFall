using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialProgressor : MonoBehaviour
{
    [SerializeField] DoorInteractable doorToUnlock;
    [SerializeField] private string promptText;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            Tutorial.Instance.SetPrompt(promptText);
            if(doorToUnlock != null)
                doorToUnlock.UnlockDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Tutorial.Instance.ProgressTutorial();
            Destroy(gameObject);
        }
    }
}
