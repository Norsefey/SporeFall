using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunPickupRoom : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;
    [SerializeField] private DoorInteractable exitDoor;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private float nextPromptDelay = 2;


    bool tutorialStarted = false;
    bool pistolIntroduced = false;

    private void Update()
    {
        if (tutorialStarted)
        {
            WaitForPickup();
        }
    }

    private void StartPickupTutorial()
    {
        if (!tutorialStarted)
        {
            tutorialStarted = true;
            StartCoroutine(IntroText());
        }
    }
    IEnumerator IntroText()
    {
        tutorialText.text = "Time to Arm yourself";

        // allow player time to spawn before starting tutorial
        yield return new WaitForSeconds(1);
        tutorialText.text = "Go Pick up a Pulse Pistol";
    }
    private void WaitForPickup()
    {
        if(manager.player.currentWeapon != null && !pistolIntroduced)
        {
            pistolIntroduced = true;
            tutorialText.text = "This is your main weapon." +
                "\n You can view your active weapon and its ammo at the Bottom Right";
            manager.player.pUI.ToggleWeaponUI(true);

            Invoke(nameof(EndTutorial), nextPromptDelay);
        }
    }

    private void EndTutorial()
    {
        tutorialText.text = "Now Lets go practice your aim in the room";
        exitDoor.UnlockDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartPickupTutorial();
            Destroy(GetComponent<BoxCollider>());
        }
    }
}
