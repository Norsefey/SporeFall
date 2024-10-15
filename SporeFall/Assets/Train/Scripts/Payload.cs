using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float defaultMoveSpeed = 2f;   // The speed of the payload's movement
    [SerializeField] private float topMoveSpeed = 4f; // When boss dies payload moves faster
    private Transform[] path;

    private int pathIndex = 0;
    private Vector3 destination;  // The target point where the payload is moving towards
    private float moveSpeed;
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;    // Maximum health of the payload
    private int currentHealth;     // Current health of the payload
    private bool isMoving = false; // Flag to control payload movement

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
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
        if (Vector3.Distance(transform.position, destination) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            
            Vector3 directionToTarget = destination - transform.position;
            if (directionToTarget != Vector3.zero) // Prevent division by zero
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2 * Time.deltaTime);
            }
        }
        else if(pathIndex < path.Length)
        {
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
