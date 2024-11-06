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
        yield return new WaitForSeconds(2);
        tutorialPopup.SetActive(true);
        tutorialText.text = "WASD to move" + "\n Shift to sprint" + "\n Space to jump";
        yield return new WaitForSeconds(5);
        tutorialText.text = "Right click to aim" + "\n Left click to shoot" + "\n R to reload";
        yield return new WaitForSeconds(6);
        tutorialText.text = "B to toggle Build Mode" + "\n Right click to preview" + "\n Left click to place";
        yield return new WaitForSeconds(7);
        tutorialText.text = "F to interact" + "\n Hold Q to drop weapon" + "\n (excluding default guns)";
        yield return new WaitForSeconds(5);
        tutorialText.text = "Esc to pause" + "\n Controls can be reviewed any time" + "\n in pause menu";
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
