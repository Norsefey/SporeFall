using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShootingRoomTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;
    [SerializeField] private DoorInteractable exitDoor;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private float nextPromptDelay = 2;

    [SerializeField] protected GameObject dummyTwoWall;

    private bool tutorialStarted = false;
    private bool dummyOneAlive = true;
    private bool dummyTwoAlive = true;
    private bool waitingForWeaponDrop = false;
    private void Update()
    {
        if (waitingForWeaponDrop)
        {
            WaitForWeaponDrop();
        }
    }

    private void StartTutorial()
    {
        tutorialStarted = true;
        if (manager.usingGamepad)
        {
            tutorialText.text = "Hold Left Trigger to aim" + "\n Right Trigger to shoot" + "\n East Button to reload";
        }
        else
        {
            tutorialText.text = "Hold Right click to aim" + "\n Left click to shoot" + "\n R to reload";
        }

        tutorialText.text += "\n Destroy the Target";
    }
    public void TargetKilled()
    {
        if (dummyOneAlive)
        {
            dummyOneAlive = false;
            EnemyDropTutorial();
        }
        else if (dummyTwoAlive)
        {
            dummyTwoAlive = false;
            DropWeaponTutorial();
        }
    }
    public void EnemyDropTutorial()
    {
        tutorialText.text = "Enemies will sometimes drop weapons, which you can pick up";
    }
    public void StartCorruptedWeaponTutorial()
    {
        StartCoroutine(CorruptedWeaponTutorial());
    }
    public IEnumerator CorruptedWeaponTutorial()
    {
        tutorialText.text = "Weapons dropped by enemies, like this one, are what we call 'corrupted'.";
        yield return new WaitForSeconds(nextPromptDelay);
        tutorialText.text = "Corrupted weapons are powerful, but fill your corruption meter over time as you hold them.";
        yield return new WaitForSeconds(nextPromptDelay);
        tutorialText.text = "The Corruption Meter can be found at the bottom left, along with your current HP and remaining lives.";
        yield return new WaitForSeconds(nextPromptDelay);
        dummyTwoWall.SetActive(false);
        tutorialText.text = "This is a charge type weapon, hold down fire to charge up a more powerful shot." + "\n Try it out on the next target";
    }

    private void DropWeaponTutorial()
    {
        waitingForWeaponDrop = true;
        tutorialText.text = "Something bad might happen if your corruption maxes out...";

        if (manager.usingGamepad)
        {
            tutorialText.text += "\n To drop a weapon, hold <b>EAST Button</b>.";
        }
        else
        {
            tutorialText.text += "\n To drop a weapon, hold <b>Q</b>.";
        }
    }

    private void WaitForWeaponDrop()
    {
        if (manager.player.pInput.dropAction.WasPerformedThisFrame())
        {
            waitingForWeaponDrop = false;
            tutorialText.text = "Your corruption decreases gradually when you aren't holding a corrupted weapon.";

            Invoke(nameof(RoomComplete), nextPromptDelay);
        }
    }

    private void RoomComplete()
    {
        tutorialText.text = "That does it for basic weapon training, Proceed to the Next Room";
        exitDoor.canOpen = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tutorialStarted)
        {
            tutorialText.text = "Head towards the target";
            Invoke(nameof(StartTutorial), nextPromptDelay);
            Destroy(GetComponent<BoxCollider>());
        }
    }
}
