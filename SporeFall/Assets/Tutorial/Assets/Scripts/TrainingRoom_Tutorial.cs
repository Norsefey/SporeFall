using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class TrainingRoom_Tutorial : MonoBehaviour
{
    private Dictionary<int, string> keyboardSteps = new Dictionary<int, string>()
    {
        {0, "<color=red>Left click (keyboard)</color> or <color=red>Start Button (controller)</color> to lock cursor and Start the Game" },
        {1, "WASD to move \n Mouse to look around \n Shift to sprint \n Space to jump"},
        {2, "Approach and Pick up your Weapon"},
        
        {3, "This is your default weapon. Your active weapon and its ammo are in the bottom right"},
        {4, "Go To Next Room" },

        {5, "Right click to aim,\n Left click to shoot,\n R to reload" },

        {6, "Enemies will sometimes drop weapons, which you can pick up, Approach the Weapon" },

        {7, "Some weapons, like this one, can be charged by holding Fire \n Try charging it"},
        {8, "Weapons dropped by enemies, are what we call 'corrupted'"},
        {9, "Corrupted weapons are powerful, but fill your corruption meter over time as you hold them"},
        {10, "The bottom left shows your extra lives, HP bar, and corruption meter" },
        {11, "Something bad might happen if your corruption maxes out... To drop a weapon, hold Q" },
        {12, "Your corruption decreases gradually when you aren't holding a corrupted weapon"},
        {13, "Continue to the next room" },
        {14, "RnD has a Upgrade for the basic pistol, \n Approach the upgrade table"},
        {15, "Unlock the Material Build Gun" },
        {16, "Along with your weapons, this will allow you can build structures to help you" },
        {17, "Press B to toggle Build Mode" },
        {18, "Use the <color=blue>Scroll Wheel</color> to view each structure. Hold Right click for a better View Point" },
        {19, "Structures cost Mycelia, which is dropped by enemies, and shown in the top left" },
        {20, "Try placing a something with left click" },
        {21, "Press F to toggle Edit Mode, which allows you to edit placed structures" },
        {22, "Move your structure by left clicking and dragging" },
        {23, "Structures can also be destroyed by holding X for a (partial if damaged) refund. \n Destroy Your Structure"},
        {24, "You can pause at any time with <color=blue>Esc</color> to review the controls if needed"},
        {25, "Basic Aegis Training Complete, Proceed to the next Room"}
    };
    private Dictionary<int, string> gamepadSteps = new Dictionary<int, string>()
    {
        {0, "<color=red>Left click (keyboard)</color> or <color=red>Start Button (controller)</color> to Start the Game" },
        {1, "Left stick to move \n Right stick to look around\" \n Press Left stick to sprint \n A to jump"},
        
        {2, "Approach and Pick up your Weapon"},
        {3, "This is your default weapon. Your active weapon and its ammo are in the bottom right"},
        {4, "Left trigger to aim\nRight trigger to shoot\n B to reload" },
        {5, "Practice Your Aim"},
        
        {6, "Enemies will sometimes drop weapons, which you can pick up, Approach the Weapon" },
        {7, "Some weapons, like this one, can be charged by holding Fire \n Try charging it"},
        {8, "Weapons dropped by enemies, are what we call 'corrupted'"},
        {9, "Corrupted weapons are powerful, but fill your corruption meter over time as you hold them"},
        {10, "The bottom left shows your extra lives, HP bar, and corruption meter" },
        {11, "Something bad might happen if your corruption maxes out... To drop a weapon, hold B" },
        {12, "Your corruption decreases gradually when you aren't holding a corrupted weapon"},
        
        {13, "Continue to the next room" },
        {14, "RnD has a Upgrade for the basic pistol, \n Approach the upgrade table"},
        {15, "Unlock the Material Build Gun" },
        {16, "Along with your weapons, this will allow you can build structures to help you" },
        
        {17, "Press Y to toggle Build Mode" },
        {18, "Use the <color=blue>L/R Top Bumpers</color> to view each structure. Hold Left trigger for a better View Point" },
        {19, "Structures cost Mycelia, which is dropped by enemies, and shown in the top left" },
        {20, "Try placing a something with Right Trigger" },
        {21, "Press X to toggle Edit Mode, which allows you to edit placed structures" },
        {22, "Move your structure by left clicking and dragging" },
        {23, "Structures can also be destroyed by holding B for a (partial if damaged) refund. \n Destroy Your Structure"},
        {24, "You can pause at any time with <color=blue>Start</color> to review the controls if needed"},
        {25, "Basic Aegis Training Complete, Proceed to the next Room"}
    };
    [SerializeField] TMP_Text tutorialText;
    private int tutotialCounter = -1;
    private bool showingKeyboardTutorial = true;
    private Coroutine delayedPrompt;

    private void Start()
    {
        // show the initial Prompt
        NextTutorialPrompt(false);

        // when the player joins, an event is played, this listens in and calls the function when the event is played
        GameManager.OnPlayerJoin += GetPlayerDevice;
    }
    private void GetPlayerDevice(int playerIndex)
    {
        // get the device the first player is currently using
        var device = GameManager.Instance.players[playerIndex].myDevice;
        
        // If using a gamepad, show gamepad tutorial
        if (device is Gamepad)
        {
            showingKeyboardTutorial = false;
        }
        else
        {
            showingKeyboardTutorial = true;
        }
        
        NextTutorialPrompt(false);
    }
    public void NextTutorialPrompt(bool timed)
    {
        if(delayedPrompt != null)
        StopCoroutine(delayedPrompt);

        tutotialCounter++;
        if(showingKeyboardTutorial)
            tutorialText.text = keyboardSteps[tutotialCounter];
        else 
            tutorialText.text = gamepadSteps[tutotialCounter];

        if (timed)
            delayedPrompt = StartCoroutine(DelayedNextPrompt());
    }
    IEnumerator DelayedNextPrompt()
    {
        yield return new WaitForSeconds(3);
        NextTutorialPrompt(false);
    }
    public void SetTutorialPrompt(string text)
    {
        tutorialText.text = text;
    }
}
