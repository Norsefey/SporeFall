using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;

    [Header("Game Objects")]
    [SerializeField] GameObject tutorialPopup;
    [SerializeField] GameObject bgImage;
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] TMP_Text continueText;
    [SerializeField] GameObject floorButton1;
    [SerializeField] GameObject floorButton2;
    [SerializeField] GameObject dummyEnemy;
    [Header("Main Variables")]
    private bool keyboardTutorial = false;
    //private bool gamepadTutorial = false;
    private bool mainLevelTutorial = false;
    public bool tutorialStarted = false;
    public bool playerActive = false;
    public bool usingKeyboard = false;
    public bool usingGamepad = false;
    public int tutorialPrompt = 0;
    private bool canProgress = false;
    public bool clickNeeded = false;
    [SerializeField] string currentScene;

    private void Awake()
    {
        Instance = this;
        if (currentScene == "Tutorial")
        {
            tutorialPopup.SetActive(true);
            floorButton1.SetActive(false);
            floorButton2.SetActive(false);
        }
        
        bgImage.SetActive(false);
        tutorialText.text = " ";
        continueText.text = " ";
    }
    // Start is called before the first frame update
    void Start()
    {
        if (currentScene == "Tutorial")
        {
            Debug.Log("Showing first prompt");
            bgImage.SetActive(true);
            tutorialText.text = "Left click (keyboard) or Start Button (controller) to lock cursor and start the game";
            continueText.text = " ";
            tutorialStarted = true;
        }

        else
        {
            StartCoroutine(InitialCooldown());
        }
        

    }

    private void Update()
    {
        
        if (playerActive == true && tutorialStarted == true)
        {
            if (currentScene == "Tutorial")
            {
                Debug.Log("Player is active and tutorial has started");

                if (usingKeyboard == true)
                {
                    Debug.Log("Starting keyboard tutorial");
                    StartKeyboardTutorial();
                    tutorialStarted = false;
                }

                else if (usingGamepad == true)
                {
                    Debug.Log("Starting gamepad tutorial");
                    StartGamepadTutorial();
                    tutorialStarted = false;
                }
            }

            else
            {
                Debug.Log("Starting main level tutorial");
                mainLevelTutorial = true;
                tutorialStarted = false;
                canProgress = true;
            }
            

        }

        #region keyboard Tutorial
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
                    floorButton1.SetActive(true);
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
                    tutorialText.text = "Enemies will sometimes drop weapons, which you can pick up with F";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 7)
                {
                    tutorialText.text = "Some weapons, like this one, can be charged by holding left click." + "\n It is also corrupted...";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                if (tutorialPrompt == 8)
                {
                    tutorialText.text = "Corrupted weapons are powerful, but fill your corruption meter over time as you hold them";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                if (tutorialPrompt == 9)
                {
                    tutorialText.text = "The bottom left shows your extra lives (blue), HP bar (green), and corruption meter (red)";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                if (tutorialPrompt == 10)
                {
                    tutorialText.text = "Something bad might happen if your corruption maxes out..." + "\n To drop a weapon, hold Q";
                    continueText.text = " ";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                if (tutorialPrompt == 11)
                {
                    tutorialText.text = "Your corruption decreases gradually when you aren't holding a corrupted weapon.";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                if (tutorialPrompt == 12)
                {
                    tutorialText.text = "Continue to the next room";
                    continueText.text = " ";
                    floorButton2.SetActive(true);
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
                    keyboardTutorial = false;
                    StartCoroutine(FinalPrompts());
                }
                
            }
        }
        #endregion

        #region main Level Tutorial
        if (mainLevelTutorial == true)
        {
            if (canProgress == true)
            {
                if (tutorialPrompt == 0)
                {
                    tutorialText.text = "It's time to take the fight to our enemies. We have a weapon to destroy them from the source...";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 1)
                {
                    tutorialText.text = "...but we need to reach that source, first. You must escort the train and defend if with your life.";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 2)
                {
                    tutorialText.text = "Those giant red pods across from you are where they'll come from. There are more pods ahead.";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 3)
                {
                    tutorialText.text = "The bar at the top shows you how many enemies and pods you have left to destroy.";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 4)
                {
                    tutorialText.text = "Set up your defenses. They'll move with the train (and be refunded if they overlap with anything).";
                    continueText.text = "(Press C to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 5)
                {
                    tutorialText.text = "When you're ready, press the Main Button to lure your enemies out of hiding.";
                    continueText.text = " ";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                else if (tutorialPrompt == 6)
                {
                    canProgress = false;
                    StartCoroutine(FinalPrompts());
                }

                else if (tutorialPrompt == 7)
                {
                    tutorialText.text = "You can also hit the Main Button to go early.";
                    continueText.text = "(Press C to close)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 8)
                {
                    canProgress = false;
                    tutorialPopup.SetActive(false);
                }

                else if (tutorialPrompt == 9)
                {
                    canProgress = false;
                    tutorialPopup.SetActive(false);
                }

                else if (tutorialPrompt == 10)
                {
                    canProgress = false;
                    tutorialPopup.SetActive(false);
                    mainLevelTutorial = false;
                }
            }
        }
        #endregion

    }

    IEnumerator InitialCooldown()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("Showing first prompt");
        tutorialPopup.SetActive(true);
        bgImage.SetActive(true);
        tutorialText.text = "Left click (keyboard) or Start Button (controller) to lock cursor and start the game";
        continueText.text = " ";
        tutorialStarted = true;
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

    public void StartBetweenWaveTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "The train will move to the next area shortly, picking up any uncollected Mycelia.";
        continueText.text = "(Press C to continue)";
        clickNeeded = true;
    }

    public void StartFinalWaveTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "This is it! Defeat the boss so we can deploy our payload and stop them for good.";
        continueText.text = "(Press C to close)";
        clickNeeded = true;
    }

    public void StartPayloadTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "Payload deployed. Leave the train be, focus on defending the payload!";
        continueText.text = "(Press C to close)";
        clickNeeded = true;
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
        yield return new WaitForSeconds(2.2f);
        tutorialText.text = "Fight off the horde";
        yield return new WaitForSeconds(2.2f);
        tutorialText.text = "Good luck!";
        yield return new WaitForSeconds(2.2f);
        tutorialPopup.SetActive(false);
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

}