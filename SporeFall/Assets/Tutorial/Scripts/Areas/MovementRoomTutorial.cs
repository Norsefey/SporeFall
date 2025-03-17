using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovementRoomTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;
    [SerializeField] private DoorInteractable exitDoor;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private float nextPromptDelay = 2;

    private int tutorialIndex = 0;
    private bool detectInput = false;

    private bool isLookCompleted = false;
    private bool isMovementCompleted = false;
    private bool isSprintCompleted = false;
    private bool isJumpCompleted = false;

    private void Update()
    {
        if (detectInput)
        {
            switch (tutorialIndex)
            {
                case 0:
                    WaitForLookInput();
                    break;
                case 1:
                    WaitForMovementInput();
                    break;
                case 2:
                    WaitForSprintInput(); 
                    break;
                case 3:
                    WaitForJumpInput();
                    break;
            }
        }
       
    }
    public void StartTutorial(PlayerManager player)
    {
        tutorialText.text = "Initiating Connection....";
        player.TogglePControl(false);
        StartCoroutine(IntroText());
    }
    IEnumerator IntroText()
    {
        // allow player time to spawn before starting tutorial
        yield return new WaitForSeconds(1);
        tutorialText.text = "Connected";
        yield return new WaitForSeconds(1);
        tutorialText.text = "Welcome";
        yield return new WaitForSeconds(1.5f);
        tutorialText.text = "Lets go over the basics";
        yield return new WaitForSeconds(1.5f);

        manager.player.TogglePControl(true);
        StartLookTutorial();
    }
    private void StartLookTutorial()
    {
        // allow time for players to read tutorial even if they are doing the action
        detectInput = false;
        if (manager.usingGamepad)
        {
            tutorialText.text = "Use Right STICK to Look around";
        }
        else
        {
            tutorialText.text = "Use Mouse to Look around";
        }
        // start listening for action input
        Invoke(nameof(ToggleStartDetectingInput), nextPromptDelay);
    }
    private void WaitForLookInput()
    {
        if(!isLookCompleted && manager.player.pInput.lookAction.IsInProgress())
        {
            detectInput = false;
            isLookCompleted = true;
            tutorialText.text = "Great Job!";
            tutorialIndex++;
            Invoke(nameof(StartMovementTutorial), nextPromptDelay);
        }
    }
    private void StartMovementTutorial()
    {
        detectInput = false;
        if (manager.usingGamepad)
        {
            tutorialText.text = "Use LEFT STICK to walk around";
        }
        else
        {
            tutorialText.text = "Use WASD to walk around";
        }
        Invoke(nameof(ToggleStartDetectingInput), nextPromptDelay);
    }
    private void WaitForMovementInput()
    {
        if (!isMovementCompleted && manager.player.pInput.moveAction.IsInProgress())
        {
            detectInput = false;
            isMovementCompleted = true;
            tutorialText.text = "Great Job!";
            tutorialIndex++;
            Invoke(nameof(StartSprintTutorial), nextPromptDelay);

        }
    }
    private void StartSprintTutorial()
    {
        detectInput = false;
        if (manager.usingGamepad)
        {
            tutorialText.text = "Press Down on LEFT STICK to Look around";
        }
        else
        {
            tutorialText.text = "Hold SHIFT to Sprint";
        }
        
        Invoke(nameof(ToggleStartDetectingInput), nextPromptDelay);
    }
    private void WaitForSprintInput()
    {
        if (!isSprintCompleted && manager.player.pInput.sprintAction.inProgress)
        {
            isSprintCompleted = true;
            tutorialText.text = "Great Job!";
            tutorialText.text += "\n <size= 15>Sprinting decreases shooting accuracy</size>";
            tutorialIndex++;
            Invoke(nameof(StartJumpTutorial), nextPromptDelay);
        }
    }
    private void StartJumpTutorial()
    {
        detectInput = false;
        if (manager.usingGamepad)
        {
            tutorialText.text = "Press SOUTH BUTTON to Look around";
        }
        else
        {
            tutorialText.text = "Press SPACEBAR to Jump";
        }
        Invoke(nameof(ToggleStartDetectingInput), nextPromptDelay);
    }
    private void WaitForJumpInput()
    {
        if (!isJumpCompleted && manager.player.pInput.jumpAction.IsInProgress())
        {
            detectInput = false;
            isJumpCompleted = true;
            tutorialText.text = "Great Job!";
            tutorialIndex++;
            Invoke(nameof(StartCompleteCourse), nextPromptDelay);
        }
    }
    private void StartCompleteCourse()
    {
        StartCoroutine(RoomComplete());
    }
    IEnumerator RoomComplete()
    {
        tutorialText.text = "You are ready to move on";
        yield return new WaitForSeconds(1.5f);
        tutorialText.text = "Proceed to the Door across the room.";
        exitDoor.canOpen = true;
    }
    private void ToggleStartDetectingInput()
    {
        detectInput = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isJumpCompleted)
            {
                tutorialText.text = "Quite skilled I see, then lets move on.";
                exitDoor.canOpen = true;
                Destroy(gameObject);
            }
        }
    }
}
