using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sherman : MonoBehaviour
{
    // Movement speed
    public float moveSpeed = 2f;

    // How fast the object changes direction
    public float turnSpeed = 1f;

    // How often to change direction
    public float changeDirectionInterval = 2f;

    private Vector3 randomDirection;

    public delegate void EnemyDeath();
    public event EnemyDeath OnEnemyDeath;

    private float maxHP;
    public float hp = 10;
    [SerializeField] private TMP_Text hpDisplay;
    void Start()
    {
        maxHP = hp;
        if (hpDisplay != null)
        {
            hpDisplay.text = hp.ToString() + "/" + maxHP.ToString();     
        }
        // Pick an initial random direction
        randomDirection = GetRandomDirection();
        
        // Start the direction-changing process
        InvokeRepeating("ChangeDirection", 0f, changeDirectionInterval);
    }

    void Update()
    {
        // Move in the current random direction
        transform.Translate(moveSpeed * Time.deltaTime * randomDirection, Space.World);


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
    public void TakeDamage(float damage)
    {
        Debug.Log("Received Damage: " + damage);
        hp -= damage;
        if(hpDisplay != null)
            hpDisplay.text = hp.ToString() + "/" + maxHP.ToString();
        // Handle taking damage
        if (hp <= 0)
        {
            Die();
        }
    }
    // add this event to enemies, call when they die, which lets this script that that enemy has been killed
    private void Die()
    {
        OnEnemyDeath?.Invoke(); // Notify the wave system that the enemy died
        Destroy(gameObject);
    }

}
