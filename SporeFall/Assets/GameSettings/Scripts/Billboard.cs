using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private BillboardUIUpdater uiContainerPlayerOne;
    [SerializeField] private BillboardUIUpdater uiContainerPlayerTwo;

    private void Start()
    {
        InitialSetup();
    }
    private void OnEnable()
    {
        SetPlayer(0);
    }
    private void OnDisable()
    {
        //Debug.Log("Removing Event Listener To On player join");
        GameManager.OnPlayerJoin -= SetPlayer;
    }
    private void InitialSetup()
    {
        if (GameManager.Instance != null)
        {
            //Debug.Log("Listening To On player join");
            GameManager.OnPlayerJoin += SetPlayer;
        }
    }
    // Call this to set which player this UI belongs to
    public void SetPlayer(int playerIndex)
    {
        if (!GameManager.Instance)
            return;
       // Debug.Log("Setting Up players");
        if (GameManager.Instance.players.Count == 1)
        {   
           // Debug.Log("Setting Up Player One");
            uiContainerPlayerOne.SetupTarget(GameManager.Instance.players[0].pCamera.transform);
           
        }
        else if (GameManager.Instance.players.Count == 2 && uiContainerPlayerTwo != null)
        {
            //Debug.Log("Setting Up Player Two");

            uiContainerPlayerTwo.gameObject.SetActive(true);
            uiContainerPlayerTwo.SetupTarget(GameManager.Instance.players[1].pCamera.transform);
        }
        else
        {
            //Debug.Log("No Players to set Up");
        }
    }
    public void RemovePlayer()
    {
        uiContainerPlayerTwo.gameObject.SetActive(false);
    }
}
