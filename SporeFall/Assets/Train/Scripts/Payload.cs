using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Payload : MonoBehaviour
{
    //[SerializeField] private WaveUI wUI;
    
    [Header("Movement")]
    [SerializeField] private float defaultMoveSpeed = 2f;   // The speed of the payload's movement
    [SerializeField] private float topMoveSpeed = 4f; // When boss dies payload moves faster
    private Transform[] path;
    private Transform papa;
    private int pathIndex = 0;
    private Vector3 destination;  // The target point where the payload is moving towards
    private float moveSpeed;
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;    // Maximum health of the payload
    private int currentHealth;     // Current health of the payload
    private bool isMoving = false; // Flag to control payload movement
    //public int payloadProgress = 0;

    [SerializeField] private TMP_Text HpDisplay;
    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
        moveSpeed = defaultMoveSpeed; // Initialize move speed
        HpDisplay.text = currentHealth.ToString() + "/" + maxHealth.ToString();

        papa = transform.parent;
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
            //payloadProgress++;
            WaveUI.Instance.DisplayWaveProgress(pathIndex);
            SetDestination();
        }
        else
        {
            isMoving = false;
            Debug.Log("ReachDestination");
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
        SetDestination();
        isMoving = true;
    }
    // Method to handle damage to the payload
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        HpDisplay.text = currentHealth.ToString() + ":" + maxHealth.ToString();
        // Check if health drops to or below zero
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Payload Destroyed");
            Destroy(gameObject);          // Destroy the payload object
        }
    }
    public void IncreaseSpeed()
    {
        moveSpeed = topMoveSpeed;
    }
}
