using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload : MonoBehaviour
{
    public Vector3 destination;  // The target point where the payload is moving towards
    public float moveSpeed = 2f;   // The speed of the payload's movement
    public int maxHealth = 100;    // Maximum health of the payload
    private int currentHealth;     // Current health of the payload

    private bool isMoving = false; // Flag to control payload movement

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
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
        if (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;  // Stop moving once the payload reaches the destination
            Debug.Log("Breach Destroyed");
        }
    }
    public void SetDestination(Vector3 position)
    {
        destination = position; 
    }
    // Method to start the payload movement
    public void StartMoving()
    {
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
}
