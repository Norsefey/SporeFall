using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;
    //[SerializeField] private TutorialControls tControl;

    [Header("Game Objects")]
    public GameObject tutorialPopup;
    [SerializeField] GameObject bgImage;
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] TMP_Text continueText;
    [SerializeField] GameObject dummyEnemy;
    public GameObject[] door;

    [Header("Main Variables")]
    public int tutorialPrompt = 0;
    //private bool tutorialActive = false;
    private bool mainLevelTutorial = false;
    public bool tutorialStarted = false;
    public bool tutorialOngoing;
    
    private bool canProgress = false;
    public bool clickNeeded = false;

    [HideInInspector] public float timer = 0f;
    private bool timerStarted = false;
    private bool timerNeeded = false;
    private float timerThreshold = 20f;
    private int timerType = 1;

    private void Awake()
    {
        Instance = this;
        if (SavedSettings.currentLevel == "Tutorial")
        {
            tutorialPopup.SetActive(true);
        }
        
        bgImage.SetActive(false);
        tutorialText.text = " ";
        continueText.text = " ";
    }
    // Start is called before the first frame update
    void Start()
    {
        //Tutorial scene has more in-depth tutorial than the one in the main level(s)
        //Checks which scene it is and determines what tutorial to play
        //Main level tutorial is on a delay, full tutorial is not
        if (SavedSettings.currentLevel == "Tutorial")
        {
            Debug.Log("Showing first prompt");
            bgImage.SetActive(true);
            tutorialText.text = "Left click (keyboard) or Start Button (controller) to lock cursor and start the game";
            continueText.text = " ";
            tutorialStarted = true;
        }
        else
        {
            if (SavedSettings.firstTimeTutorial)
            {
                timerNeeded = true;
            }
        }
    }

    private void Update()
    {
        if (timer < timerThreshold && timerNeeded)
        {
            timer += Time.deltaTime;
        }
        
        if (timer >= timerThreshold && timerStarted == false)
        {
            if (timerType == 1 && SavedSettings.firstTimeTutorial)
            {
                //Tutorial prompts when loading into level for first time
                timerStarted = true;
                Debug.Log("Showing first prompt");
                tutorialPopup.SetActive(true);
                bgImage.SetActive(true);
                tutorialText.text = "Left click (keyboard) or Start Button (controller) to lock cursor and start the game";
                continueText.text = " ";
                tutorialStarted = true;
                tutorialOngoing = true;
                timerNeeded = false;
            }
            
            else if (timerType == 2)
            {
                //Tutorial prompts between waves 1 and 2
                if (timerThreshold == 5f)
                {
                    timerThreshold = 10f;
                    tutorialText.text = "You can also hit the Main Button to go early.";
                }

                else if (timerThreshold == 10f)
                {
                    timerThreshold = 15f;
                    tutorialText.text = "You can press " + TutorialControls.Instance.skipInput + " to skip the cutscene if you'd like.";
                }

                else if (timerThreshold == 15f)
                {
                    canProgress = false;
                    tutorialOngoing = false;
                    tutorialPopup.SetActive(false);
                    timerNeeded = false;
                    timerStarted = true;
                }
                
            }

        }



        if (TutorialControls.Instance != null && TutorialControls.Instance.playerActive == true && tutorialStarted == true && TutorialControls.Instance.controlsSet)
        {
            if (SavedSettings.currentLevel == "Tutorial")
            {
                //Tutorial displays different controls depending on what device player is using
                
                if (tutorialStarted == true)
                {
                    tutorialStarted = false;
                    StartTutorial();
                }
            }
            else
            {
                Debug.Log("Starting main level tutorial");
                if (TutorialControls.Instance.usingKeyboard == true)
                {
                    mainLevelTutorial = true;
                    tutorialStarted = false;
                    canProgress = true;
                }

                else if (TutorialControls.Instance.usingXbox == true)
                {
                    mainLevelTutorial = true;
                    tutorialStarted = false;
                    canProgress = true;
                }
                
                else if (TutorialControls.Instance.usingPlaystation == true)
                {
                    mainLevelTutorial = true;
                    tutorialStarted = false;
                    canProgress = true;
                }
            }
            

        }

        //this doesn't seem to be being used right now?
        #region first Tutorial
        //if (tutorialActive == true)
        //{
        //    if (canProgress == true)
        //    {
        //        if (tutorialPrompt == 1)
        //        {
        //            tutorialText.text = "Pick up your weapon.";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 2)
        //        {
        //            tutorialText.text = "This is your default weapon. Your active weapon and its ammo are in the bottom right.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 3)
        //        {
        //            tutorialText.text = aimInput + " to aim," + "\n " + shootInput + " to shoot," + "\n " + reloadInput + " to reload.";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 4)
        //        {
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 5)
        //        {
        //            tutorialText.text = "Destroy the dummy.";
        //            continueText.text = " ";
                    
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 6)
        //        {
        //            tutorialText.text = "Enemies will sometimes drop weapons, which you can pick up with " + pickupInput + ".";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 7)
        //        {
        //            tutorialText.text = "Some weapons, like this one, can be charged by holding " + shootInput + "." + "\n Try charging it now.";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 8)
        //        {
        //            tutorialText.text = "Weapons dropped by enemies, like this one, are what we call 'corrupted'.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 9)
        //        {
        //            tutorialText.text = "Corrupted weapons are powerful, but fill your corruption meter over time as you hold them.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 10)
        //        {
        //            tutorialText.text = "The bottom left shows your extra lives (blue), HP bar (green), and corruption meter (red).";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 11)
        //        {
        //            tutorialText.text = "Something bad might happen if your corruption maxes out... To drop a weapon, hold " + dropInput + ".";
        //            continueText.text = " ";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 12)
        //        {
        //            tutorialText.text = "Your corruption decreases gradually when you aren't holding a corrupted weapon.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 13)
        //        {
        //            tutorialText.text = "Continue to the next room.";
        //            continueText.text = " ";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 14)
        //        {
        //            tutorialText.text = "Along with your weapons, you can build structures to help you fight.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 15)
        //        {
        //            tutorialText.text = "Press " + buildInput + " to toggle Build Mode.";
        //            continueText.text = " ";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 16)
        //        {
        //            tutorialText.text = "Use the " + scrollInput + " to view each structure. " + aimInput + " to preview placement at a greater distance.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 17)
        //        {
        //            tutorialText.text = "Structures cost Mycelia, which is dropped by enemies, and shown in the top left.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 18)
        //        {
        //            tutorialText.text = "Try placing a turret with " + shootInput + ".";
        //            continueText.text = " ";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 19)
        //        {
        //            tutorialText.text = "Press " + pickupInput + " to toggle Edit Mode, which allows you to edit placed structures.";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 20)
        //        {
        //            tutorialText.text = "Look at the turret, and move it by holding " + shootInput + " and dragging.";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 21)
        //        {
        //            tutorialText.text = "Structures can also be destroyed by holding " + destroyInput + " for a (partial if damaged) refund. Destroy the turret.";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 22)
        //        {
        //            tutorialText.text = "You can pause at any time with " + pauseInput + " to review the controls if needed.";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 23)
        //        {
        //            tutorialText.text = "Great, that should be all you need to know for now. Are you ready for the real deal?";
        //            continueText.text = "(Press " + continueInput + " to continue)";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //            clickNeeded = true;
        //        }

        //        else if (tutorialPrompt == 24)
        //        {
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }

        //        else if (tutorialPrompt == 25)
        //        {
        //            tutorialText.text = "You better be, we're under attack! Quick, get to the train!";
        //            continueText.text = " ";
        //            Debug.Log("Progressing tutorial");
        //            canProgress = false;
        //        }
                
        //    }
        //}
        #endregion

        #region main Level Tutorial
        if (mainLevelTutorial == true)
        {
            if (canProgress == true)
            {
                if (tutorialPrompt == 0)
                {
                    tutorialText.text = "It's time to take the fight to our enemies. We have a weapon to destroy them from the source...";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 1)
                {
                    tutorialText.text = "...but we need to reach that source, first. You must escort the train and defend if with your life.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 2)
                {
                    tutorialText.text = "Those giant red pods across from you are where they'll come from. There are more pods ahead.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 3)
                {
                    tutorialText.text = "The bar at the top shows you how many enemies and pods you have left to destroy.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 4)
                {
                    tutorialText.text = "Set up your defenses. They'll move with the train (and be refunded if they overlap with anything).";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 5)
                {
                    tutorialText.text = "When you're ready, press the Main Button in the middle of the train to lure out your enemies.";
                    continueText.text = " ";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                else if (tutorialPrompt == 6)
                {
                    canProgress = false;
                    tutorialOngoing = false;
                    StartCoroutine(FinalPrompts());
                    SavedSettings.firstTimeTutorial = false;
                }
            }
        }
        #endregion

    }



    private void StartTutorial()
    {
        Debug.Log("Keyboard tutorial has started");
        tutorialText.text = TutorialControls.Instance.moveInput + " to move \n " + TutorialControls.Instance.lookInput + " to look around" + "\n " + TutorialControls.Instance.sprintInput + " to sprint" + "\n " + TutorialControls.Instance.jumpInput + " to jump";
        //tutorialActive = true;
    }

    public void StartBetweenWaveTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialOngoing = true;
        tutorialText.text = "The train will move to the next area shortly, picking up any uncollected Mycelia.";
        continueText.text = " ";
        timerNeeded = true;
        timerStarted = false;
        timerThreshold = 5f;
        timer = 0f;
        timerType = 2;
    }

    public void StartFinalWaveTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "This is it! Defeat the boss so we can deploy our payload and stop them for good.";
        continueText.text = " ";
        StartCoroutine(ClosePrompts());
    }

    public void StartPayloadTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "Payload deployed. Leave the train be, focus on defending the payload!";
        continueText.text = " ";
        StartCoroutine(ClosePrompts());
        mainLevelTutorial = false;
        
    }

    public void ProgressTutorial()
    {
        StartCoroutine(NextPrompt());
    }
    
    IEnumerator NextPrompt()
    {
        //Slight delay so doesn't happen twice
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

    IEnumerator ClosePrompts()
    {
        yield return new WaitForSeconds(10f);
        tutorialPopup.SetActive(false);
    }
}
