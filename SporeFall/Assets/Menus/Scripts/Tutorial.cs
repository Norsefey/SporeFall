using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

    [SerializeField] GameObject tutorialPopup;
    [SerializeField] TMP_Text tutorialText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartTutorial());
    }

    
    IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(5);
        tutorialPopup.SetActive(true);
        tutorialText.text = "WASD to move" + "\n Shift to sprint" + "\n Space to jump";
        yield return new WaitForSeconds(4);
        tutorialText.text = "Right click to aim" + "\n Left click to shoot" + "\n R to reload";
        yield return new WaitForSeconds(4);
        tutorialText.text = "B to toggle Build Mode";
        yield return new WaitForSeconds(4);
        tutorialText.text = "F to interact" + "\n Hold Q to drop weapon" + "\n (excluding default and build guns)";
        yield return new WaitForSeconds(4);
        tutorialText.text = "Esc to pause" + "\n Controls can be reviewed any time" + "\n in pause menu";
        yield return new WaitForSeconds(4);
        tutorialText.text = "Find the button in the middle" + "\n of the train and press it" + "\n to start the next wave";
        yield return new WaitForSeconds(4);
        tutorialText.text = "Defend the train";
        yield return new WaitForSeconds(2);
        tutorialText.text = "Fight off the horde";
        yield return new WaitForSeconds(2);
        tutorialText.text = "Good luck!";
        yield return new WaitForSeconds(3);
        tutorialPopup.SetActive(false);

    }
}
