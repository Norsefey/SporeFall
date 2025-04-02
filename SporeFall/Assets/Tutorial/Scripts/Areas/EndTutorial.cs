using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;
    [SerializeField] private Transform elevatorStandPoint;
    [SerializeField] private Transform elevator;
    [SerializeField] private GameObject inputManager;

    [SerializeField] private Transform p1Point, p2Point;
    private void StartEndSequance()
    {
        int count = 0;
        foreach (PlayerManager player in GameManager.Instance.players)
        {
            player.TogglePControl(false);
            player.TogglePCamera(false);
            ///player.transform.SetParent(elevator);
            if(count == 0)
                player.MovePlayerTo(p1Point.position);
            else
                player.MovePlayerTo(p2Point.position);

            count++;
        }

        
        inputManager.SetActive(false);
        GetComponent<Animator>().SetTrigger("PlayFinal");
    }

    public void DisableUI()
    {
        manager.player.gameObject.SetActive(false);
        GameManager.Instance.gameUI.gameObject.SetActive(false);
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(0);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartEndSequance();
        }
    }
}
