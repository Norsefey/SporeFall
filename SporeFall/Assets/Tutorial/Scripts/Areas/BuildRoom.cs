using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildRoom : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;
    [SerializeField] private DoorInteractable exitDoor;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private float nextPromptDelay = 2;
    [SerializeField] private Interactables upgradeZone;

    private float currentMycelia = 0;
    private int tutorialIndex = 0;
    private bool tutorialStarted = false;

    private void Update()
    {

        if (tutorialStarted)
        {
            switch (tutorialIndex)
            {
                case 0:
                    WaitForBuildInput();
                    break;
                case 1:
                    WaitForStructureSwitch();
                    break;
                case 2:
                    WaitForStrucPlace();
                    break;
                case 3:
                    WaitForEditToggle();
                    break;
                case 4:
                    WaitForStrucMove();
                    break;
                case 5:
                    WaitForRecycle();
                    break;
            }
        }

  
    }

    private void StartBuildingTutorial()
    {
        tutorialText.text = "Lets upgrade the Pulsar Pistol with the latest invention from R&D"
                                + "\n Approach the Upgrade Table";
    }

    public void UnlockBuildMode()
    {
        manager.player.canBuild = true;
        manager.player.pInput.ToggleUpgradeMenu(false);
        GameManager.Instance.DecreaseMycelia(100);
        GameManager.Instance.gameUI.ToggleUpgradeMenu(false);
        upgradeZone.DestroyIntractable();

        BuildModeToggleTutorial();
    }
    private void BuildModeToggleTutorial()
    {
        if (manager.usingGamepad)
        {
            tutorialText.text = "Press <b>NORTH Button</b> to toggle Build Mode";
        }
        else
        {
            tutorialText.text = "Press <b>B</b> to toggle Build Mode";
        }
        tutorialStarted = true;
    }
    private void WaitForBuildInput()
    {
        if(manager.player.pInput.buildModeAction.WasPerformedThisFrame())
        {
            tutorialIndex++;
            StructureSwitchTutotial();
        }
    }
    private void StructureSwitchTutotial()
    {
        if (manager.usingGamepad)
        {
            tutorialText.text = "Use the <b>R/L Shoulder</b> Buttons to view each structure." +
                "\n Hold <b>Left Trigger</b> to preview placement at a greater distance.";
        }
        else
        {
            tutorialText.text = "Use the Mouse-Wheel to view each structure. " +
                "\n Hold <b>Right click</b> to preview placement at a greater distance.";
        }
    }
    private void WaitForStructureSwitch()
    {
        if (manager.player.pInput.changeStructAction.WasPerformedThisFrame())
        {
            tutorialIndex++;
            StartCoroutine(PlaceStructureTutorial());
        }
    }
    IEnumerator PlaceStructureTutorial()
    {
        tutorialText.text = "Structures cost Mycelia, " +
            "\n which is dropped by enemies, and shown in the top left.";
        yield return new WaitForSeconds(nextPromptDelay);

        if (manager.usingGamepad)
        {
            tutorialText.text = "Try placing a structure on the platform by pressing <b>Right Trigger</b>";
        }
        else
        {
            tutorialText.text = "Try placing a structure on the platform by <b>left clicking</b>";
        }
        currentMycelia = GameManager.Instance.Mycelia;
    }
    private void WaitForStrucPlace()
    {
        if (GameManager.Instance.Mycelia < currentMycelia)
        {
            tutorialIndex++;
            currentMycelia = GameManager.Instance.Mycelia;
            StartEditModeTutorial();
        }
    }
    private void StartEditModeTutorial()
    {
        if (manager.usingGamepad)
        {
            tutorialText.text = "Press <b>WEST Button</b> to toggle Edit Mode, which allows you to edit placed structures.";
        }
        else
        {
            tutorialText.text = "Press <b>F</b> to toggle Edit Mode, which allows you to edit placed structures.";
        }
    }
    private void WaitForEditToggle()
    {
        if (manager.player.pInput.editModeAction.WasPerformedThisFrame())
        {
            tutorialIndex++;
            if (manager.usingGamepad)
            {
                tutorialText.text = "Look at the turret, and move it by holding <b>Right Trigger</b>";
            }
            else
            {
                tutorialText.text = "Look at the turret, and move it by left clicking and dragging.";
            }
        }
    }
    private void WaitForStrucMove()
    {
        if (manager.player.bGun.movingStructure)
        {
            tutorialIndex++;
            if (manager.usingGamepad)
            {
                tutorialText.text = "Structures can also be Recycled by holding <b>EAST Button</b> for a (partial if damaged) refund." +
                   " \n Recycle the Structure.";
            }
            else
            {
                tutorialText.text = "Structures can also be Recycled by holding X for a (partial if damaged) refund." +
                    " \n Recycle the Structure.";
            }
        }
    }

    private void WaitForRecycle()
    {
        if (manager.player.pInput.destroyStructAction.WasPerformedThisFrame())
        {
            tutorialIndex++;
            if (manager.usingGamepad)
            {
                tutorialText.text = "You can pause at any time with <b>Start Button</b> to review the controls if needed.";

            }
            else
            {
                tutorialText.text = "You can pause at any time with Esc to review the controls if needed.";
            }
            manager.player.ToggleBuildMode();
            StartCoroutine(TutorialComplete());
        }
    }

    private IEnumerator TutorialComplete()
    {
        yield return new WaitForSeconds(nextPromptDelay);
        tutorialText.text = "That completes Aegis Training 101";
        exitDoor.canOpen = true;
        yield return new WaitForSeconds(nextPromptDelay);
        tutorialText.text = "Please proceed to the next room";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartBuildingTutorial();
            Destroy(GetComponent<BoxCollider>());
        }
    }
}
