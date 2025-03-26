using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject introText;
    [SerializeField] private GameObject tutorialText;
    [SerializeField] private MovementRoomTutorial moveTut;
    [SerializeField] private GameObject worldCamera;
    public bool usingGamepad = false;
    public PlayerManager player;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.OnPlayerJoin += GetPlayerDevice;
    }
    private void GetPlayerDevice(int playerIndex)
    {
        player = GameManager.Instance.players[playerIndex];
        var device = player.myDevice;

        if (device is Gamepad)
        {
            usingGamepad = true;
        }
        else
        {
            usingGamepad = false;
        }
        introText.SetActive(false);
        tutorialText.SetActive(true);
        worldCamera.SetActive(false);
        moveTut.StartTutorial(player);
        // remove listener, since it stays even when changing scenes,
        // which leads to errors as this scripts doesn't exist in other scenes
        GameManager.OnPlayerJoin -= GetPlayerDevice;
    }

}
