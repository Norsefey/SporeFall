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

    public bool isRobertSpawned = false;
    public bool isRobertDeadFast = false;

    [HideInInspector] public float timer = 0f;
    private bool timerStarted = false;
    private bool timerNeeded = false;
    private float timerThreshold = 20f;
    private int timerType = 1;

    private void Awake()
    {
        Instance = this;
        //These get reset for playtesting/showcase purposes
        SavedSettings.firstBetweenTutorial = true;
        SavedSettings.robertSpawned = false;
        SavedSettings.firstRobertKill = false;
        bgImage.SetActive(false);
        tutorialText.text = " ";
        continueText.text = " ";
    }
    // Start is called before the first frame update
    void Start()
    {
        if ( SavedSettings.currentLevel != "Training")
        {
            timerNeeded = true;
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
            if (timerType == 1)
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
                    tutorialText.text = "You can also hit the Train Button to go early.";
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

            else if (timerType == 3 && timerThreshold == 10f)
            {
                timerStarted = true;
                timerNeeded = false;
                tutorialPopup.SetActive(false);
                bgImage.SetActive(false);
            }

        }

        if (isRobertSpawned && SavedSettings.robertSpawned == false)
        {
            SavedSettings.robertSpawned = true;
            timerType = 3;
            timer = 0f;
            timerThreshold = 15f;
            timerStarted = false;
            timerNeeded = true;
            tutorialPopup.SetActive(true);
            bgImage.SetActive(true);
            tutorialText.text = "The train won't move with that thing around! Take it down, or hit the Train Button to leave it behind.";
            continueText.text = " ";
        }



        if (TutorialControls.Instance != null)
        {

            if (TutorialControls.Instance.playerActive == true)
            {
                if (tutorialStarted == true)
                {
                    if (TutorialControls.Instance.controlsSet)
                    {
                        Debug.Log("Controls are set");
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
            }
        }
           

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
                    tutorialText.text = "Structures cost energy and mycelia to build. Both are shown in the top left.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 5)
                {
                    tutorialText.text = "The train provides energy to structures, but it only has so much energy to spare.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 6)
                {
                    tutorialText.text = "Your enemies drop mycelia upon death, dense spores that can be used for construction.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 7)
                {
                    tutorialText.text = "Set up your defenses. They'll be returned to you and refunded when the train moves.";
                    continueText.text = $"(Press {TutorialControls.Instance.continueInput} to continue)";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                    clickNeeded = true;
                }

                else if (tutorialPrompt == 8)
                {
                    tutorialText.text = "When you're ready, press the button near the middle of the train to lure out your enemies.";
                    continueText.text = " ";
                    Debug.Log("Progressing tutorial");
                    canProgress = false;
                }

                else if (tutorialPrompt == 9)
                {
                    canProgress = false;
                    tutorialOngoing = false;
                    StartCoroutine(FinalPrompts());
                }
            }
        }
        #endregion

    }


    public void StartBetweenWaveTutorial()
    {
        Debug.Log("Starting between wave tutorial");
        tutorialPopup.SetActive(true);
        bgImage.SetActive(true);
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
        StartCoroutine(ClosePrompts(10f));
    }

    public void StartPayloadTutorial()
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = "Payload deployed. Leave the train be, focus on defending the payload!";
        continueText.text = " ";
        StartCoroutine(ClosePrompts(10f));
        mainLevelTutorial = false;
        
    }

    public void ProgressTutorial()
    {
        StartCoroutine(NextPrompt());
    }

    public void RobertKillPrompts()
    {
        tutorialPopup.SetActive(true);
        bgImage.SetActive(true);
        continueText.text = " ";
        if (GameManager.Instance.waveManager.skipWindow)
        {
            tutorialText.text = "Nice work! The train will move on shortly.";
            StartCoroutine(ClosePrompts(5f));
        }

        else
        {
            tutorialText.text = "Nice work! When you're ready to move on, hit the Train Button.";
            StartCoroutine(ClosePrompts(15f));
        }

        
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

    IEnumerator ClosePrompts(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        tutorialPopup.SetActive(false);
    }
}
