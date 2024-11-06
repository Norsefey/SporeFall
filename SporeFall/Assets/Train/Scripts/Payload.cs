using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Payload : MonoBehaviour
{
    //[SerializeField] private WaveUI wUI;
    
    [Header("Movement")]
    [SerializeField] private float defaultMoveSpeed = 2f;   // The speed of the payload's movement
    [SerializeField] private float topMoveSpeed = 4f; // When boss dies payload moves faster
    [SerializeField] private Transform[] path;
    private Transform papa;
    private int pathIndex = 0;
    private Vector3 destination;  // The target point where the payload is moving towards
    private float moveSpeed;  
    private bool isMoving = false; // Flag to control payload movement
    private PayloadHP hp;
    private void Start()
    {
        hp = GetComponent<PayloadHP>();
        hp.SetManager(this);
        moveSpeed = defaultMoveSpeed; // Initialize move speed
    }
    private void Update()
    {
        if (isMoving)
        {
            MoveTowardsDestination();  // Move the payload if it's set to move
        }
    }
    // Method to move the payload towards the destination
    private void MoveTowardsDestination()
    {
        // If the payload hasn't reached the destination, keep moving towards it
        if (Vector3.Distance(papa.transform.position, destination) > 0.5f)
        {
            papa.transform.position = Vector3.MoveTowards(papa.transform.position, destination, moveSpeed * Time.deltaTime);
            
            Vector3 directionToTarget = destination - papa.transform.position;
            if (directionToTarget != Vector3.zero) // Prevent division by zero
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                papa.transform.rotation = Quaternion.Slerp(papa.transform.rotation, targetRotation, 2 * Time.deltaTime);
            }
        }
        else if(pathIndex < path.Length)
        {
            WaveUI.Instance.DisplayWaveProgress(pathIndex);
            SetDestination();
        }
        else
        {
            WaveUI.Instance.DisplayWaveProgress(pathIndex);
            isMoving = false;
            StartCoroutine(WinLevel());
        }
    }
    public void SetDestination()
    {
        destination = path[pathIndex].position;
        pathIndex++;
    }
    // Method to start the payload movement
    public void StartMoving(Transform[] payloadPath)
    {
        path = payloadPath;
        papa = transform.parent;
        SetDestination();
        isMoving = true;
    }
    public void IncreaseSpeed()
    {
        moveSpeed = topMoveSpeed;
    }
    public void DestroyPayload()
    {
        // load death screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneTransitioner.Instance.LoadLoseScene();
    }
    private IEnumerator WinLevel()
    {
        StartCoroutine(GameManager.Instance.WaveManager.DestroyShroomPod(waitTime: 4f));
        yield return new WaitForSeconds(4f);
        SceneTransitioner.Instance.LoadWinScene();
    }
}
