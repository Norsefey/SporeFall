using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;

    [SerializeField] GameObject tutorialPopup;
    [SerializeField] GameObject bgImage;
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] TMP_Text continueText;
    [SerializeField] GameObject floorButton;
    private bool keyboardTutorial = false;
    public bool tutorialStarted = false;
    public bool firstCorruptionPickup = false;
    public bool playerActive = false;
    //private bool gamepadTutorial = false;
    public bool usingKeyboard = false;
    public bool usingGamepad = false;
    private int tutorialPrompt = 0;
    private bool canProgress = false;
    public bool clickNeeded = false;

    private void Awake()
    {
        Instance = this;
        tutorialPopup.SetActive(true);
        bgImage.SetActive(false);
        floorButton.SetActive(false);
        tutorialText.text = " ";
        continueText.text = " ";
    }
    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Showing first prompt");
        bgImage.SetActive(true);
        tutorialText.text = "Left click (keyboard) or Start Button (controller) to lock cursor and start the game";
        continueText.text = " ";
        tutorialStarted = true;

    }

    private void Update()
    {
        
        if (playerActive == true && tutorialStarted == true)
        {
            Debug.Log("Player is active and tutorial has started");
            
            if (usingGamepad == true)
            {
                Debug.Log("Starting gamepad tutorial");
                StartGamepadTutorial();
                tutorialStarted = false;
            }

            if (usingKeyboard == true)
            {
                Debug.Log("Starting keyboard tutorial");
                StartKeyboardTutorial();
                tutorialStarted = false;
            }

        }

        //tutorialText.text = "Right click to aim" + "\n Left click to shoot" + "\n R to reload";
        //tutorialText.text = "HP and extra lives are in bottom left" + "\n  Current gun and ammo are in bottom right";

        if (keyboardTutorial == true)
        {
            if (canProgress == true)
            {
                if (tutorialPrompt == 1)
                {
                    tutorialText.text = "Pick up your weapon from the table using F";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 2)
                {
                    tutorialText.text = "This is your default weapon. Your active weapon and its ammo are in the bottom right.";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                if (tutorialPrompt == 3)
                {
                    tutorialText.text = "Right click to aim" + "\n Left click to shoot" + "\n R to reload";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                if (tutorialPrompt == 4)
                {
                    tutorialText.text = "Continue to the next room";
                    continueText.text = " ";
                    floorButton.SetActive(true);
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 5)
                {
                    tutorialText.text = "Destroy the dummy";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 6)
                {
                    tutorialText.text = "Q/E to change structure" + "\n Structures cost Mycelia," + "\n which is dropped by enemies";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 7)
                {
                    tutorialText.text = "F to toggle Edit Mode" + "\n and look at the structure you want to edit";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 8)
                {
                    tutorialText.text = "Left click to move the structure" + "\n X to destroy the structure" + "\n for a partial refund";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 9)
                {
                    tutorialText.text = "When not in Build Mode," + "\n  F to press buttons and pick up weapons dropped by enemies";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 10)
                {
                    tutorialText.text = "When you are ready to start the next wave," + "\n find and press the button in the middle of the train";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 11)
                {
                    tutorialText.text = "Esc to pause" + "\n Controls can be reviewed any time in pause menu";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }
                
                if (tutorialPrompt == 12)
                {
                    StartCoroutine(FinalPrompts());
                }
                
            }
        }

        if (firstCorruptionPickup == true)
        {
            firstCorruptionPickup = false;
            StartCoroutine(CorruptionTutorial());
        }

        
    }

    public void StartKeyboardTutorial()
    {
        Debug.Log("Keyboard tutorial has started");
        tutorialText.text = "WASD to move" + "\n Shift to sprint" + "\n Space to jump";
        keyboardTutorial = true;
    }

    public void StartGamepadTutorial()
    {
        //tutorialText.text = "Left Stick to move" + "\n Hold L3 to sprint" + "\n A to jump";
        //continueText.text = "(Press C to continue)";
        //StartCoroutine(Cooldown());
        //gamepadTutorial = true;
        //tutorialPrompt++;
        StartCoroutine(TempGamepadTutorial());
    }

    public void StartFinalWaveTutorial()
    {
        StartCoroutine(FinalWaveTutorial());
    }

    public void ProgressTutorial()
    {
        StartCoroutine(NextPrompt());
    }

    IEnumerator NextPrompt()
    {
        yield return new WaitForSeconds(.02f);
        tutorialPrompt++;
        canProgress = true;
        clickNeeded = false;
    }

    IEnumerator FinalPrompts()
    {
        continueText.text = " ";
        tutorialText.text = "Defend the train";
        yield return new WaitForSeconds(2);
        tutorialText.text = "Fight off the horde";
        yield return new WaitForSeconds(2);
        tutorialText.text = "Good luck!";
        yield return new WaitForSeconds(2);
        tutorialPopup.SetActive(false);
        keyboardTutorial = false;
    }

    IEnumerator TempGamepadTutorial()
    {
        continueText.text = " ";
        tutorialText.text = "Left Stick to move" + "\n Hold L3 to sprint" + "\n A to jump";
        yield return new WaitForSeconds(6);
        tutorialText.text = "Left trigger to aim" + "\n Right trigger to shoot" + "\n B to reload";
        yield return new WaitForSeconds(6);
        tutorialText.text = "Y to toggle Build Mode" + "\n Left trigger to preview" + "\n Right trigger to place";
        yield return new WaitForSeconds(6);
        tutorialText.text = "X to interact" + "\n Hold B to drop weapon" + "\n (excluding default guns)";
        yield return new WaitForSeconds(6);
        tutorialText.text = "Find the button" + "\n in the middle of the train" + "\n  and press it" + "\n to start the next wave";
        yield return new WaitForSeconds(6);
        tutorialText.text = "Defend the train";
        yield return new WaitForSeconds(2);
        tutorialText.text = "Fight off the horde";
        yield return new WaitForSeconds(2);
        tutorialText.text = "Good luck!";
        yield return new WaitForSeconds(2);
        tutorialPopup.SetActive(false);
    }

    IEnumerator CorruptionTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "You just picked up a corrupted weapon!" + "\n These weapons are powerful...";
        continueText.text = " ";
        yield return new WaitForSeconds(7);
        tutorialText.text = "...but raise your corruption meter," + "\n beneath your HP bar," + "\n over time";
        yield return new WaitForSeconds(7);
        tutorialText.text = "You can drop corrupted weapons" + "\n at any time" + "\n by holding Q";
        yield return new WaitForSeconds(7);
        tutorialPopup.SetActive(false);
    }

    IEnumerator FinalWaveTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "This is the final wave!" + "\n Defeat the boss, and a payload will spawn";
        continueText.text = " ";
        yield return new WaitForSeconds(7);
        tutorialText.text = "Don't worry about the train, protect the payload and escort it to its destination";
        yield return new WaitForSeconds(7);
        tutorialPopup.SetActive(false);
    }
}
