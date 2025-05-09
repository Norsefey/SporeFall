using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SavedSettings
{

    //Handles settings that should be kept between scenes, like camera sensitivity

    public static float mouseCamSensitivity = 25;
    public static float gamepadHorCamSensitivity = 250;
    public static float gamepadVertCamSensitivity = 200;

    public static float mouseCamSensitivity2 = 25;
    public static float gamepadHorCamSensitivity2 = 250;
    public static float gamepadVertCamSensitivity2 = 200;

    public static bool firstOpenedGame = true;
    public static bool firstTutorialQuestion = true;
    //public static bool firstTimeTutorial = true;
    public static bool firstBetweenTutorial = true;
    //public static bool firstBossTutorial = true;
    //public static bool firstPayloadTutorial = true;
    public static bool firstCompendiumQuestion = true;
    public static bool robertSpawned = false;
    public static bool firstRobertKill = false;

    public static string currentLevel;

}
