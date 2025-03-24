using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;
    [SerializeField] private Transform elevatorStandPoint;
    [SerializeField] private Transform elevator;
    private void StartEndSequance()
    {
        manager.player.TogglePControl(false);
        manager.player.TogglePCamera(false);
        manager.player.transform.SetParent(elevator);

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
