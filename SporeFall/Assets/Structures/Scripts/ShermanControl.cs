using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShermanControl : MonoBehaviour
{
    // Movement speed
    public float moveSpeed = 2f;

    // How fast the object changes direction
    public float turnSpeed = 1f;

    // How often to change direction
    public float changeDirectionInterval = 2f;

    // Damage dealt to enemies on contact
    public int damageAmount = 100;

    public string enemyTag = "Enemy";

    private Vector3 randomDirection;

    void Start()
    {
        // Pick an initial random direction
        //randomDirection = GetRandomDirection();
        // allow sherman to fly out of house
        randomDirection = transform.forward;
        // Start the direction-changing process
        InvokeRepeating("ChangeDirection", 1f, changeDirectionInterval);
    }

    void Update()
    {
        // Move in the current random direction
        transform.Translate(randomDirection * moveSpeed * Time.deltaTime, Space.World);

        // Rotate smoothly towards the desired direction
        Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    // This function generates a random direction
    private Vector3 GetRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        Vector3 randomDir = new Vector3(randomX, 0, randomZ).normalized; // Make sure the direction is normalized
        return randomDir;
    }

    // Change direction at intervals
    private void ChangeDirection()
    {
        randomDirection = GetRandomDirection();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(enemyTag))
        {
            // Try to get the EnemyHealth script on the object and deal damage
            EnemyControls hp = collision.gameObject.GetComponent<EnemyControls>();
            if (hp != null)
            {
                hp.TakeDamage(damageAmount);  // Apply 100 damage to the enemy
                Destroy(gameObject);
            }
        }
    }
}