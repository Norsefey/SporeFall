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
    private bool keyboardTutorial = false;
    public bool tutorialStarted = false;
    //private bool gamepadTutorial = false;
    private int tutorialPrompt = 0;

    private void Awake()
    {
        Instance = this;
        tutorialPopup.SetActive(true);
        bgImage.SetActive(false);
        tutorialText.text = " ";
        continueText.text = " ";
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FirstTutorialPopup());
    }

    private void Update()
    {
        if (keyboardTutorial == true)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (tutorialPrompt == 1)
                {
                    tutorialText.text = "Right click to aim" + "\n Left click to shoot" + "\n R to reload";
                    Debug.Log("Progressing tutorial");
                    StartCoroutine(Cooldown());
                }

                if (tutorialPrompt == 2)
                {
                    tutorialText.text = "B to toggle Build Mode" + "\n Right click to preview" + "\n Left click to place";
                    Debug.Log("Progressing tutorial");
                    StartCoroutine(Cooldown());
                }

                if (tutorialPrompt == 3)
                {
                    tutorialText.text = "F to interact" + "\n Hold Q to drop weapon" + "\n (excluding default guns)";
                    Debug.Log("Progressing tutorial");
                    StartCoroutine(Cooldown());
                }

                if (tutorialPrompt == 4)
                {
                    tutorialText.text = "Esc to pause" + "\n Controls can be reviewed any time" + "\n in pause menu";
                    Debug.Log("Progressing tutorial");
                    StartCoroutine(Cooldown());
                }

                if (tutorialPrompt == 5)
                {
                    tutorialText.text = "Find the button" + "\n in the middle of the train" + "\n  and press it" + "\n to start the next wave";
                    Debug.Log("Progressing tutorial");
                    StartCoroutine(Cooldown());
                }
                
                if (tutorialPrompt == 6)
                {
                    StartCoroutine(FinalPrompts());
                }
                
            }
        }

        
    }

    public void StartKeyboardTutorial()
    {
        tutorialText.text = "WASD to move" + "\n Shift to sprint" + "\n Space to jump";
        continueText.text = "(Press C to continue)";
        keyboardTutorial = true;
        tutorialPrompt++;
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

    IEnumerator FirstTutorialPopup()
    {
        yield return new WaitForSeconds(2);
        bgImage.SetActive(true);
        tutorialText.text = "Left click/Start Button to lock cursor" + "\n and start the game";
        continueText.text = " ";
        tutorialStarted = true;
    }
    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1);
        tutorialPrompt++;
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
}
