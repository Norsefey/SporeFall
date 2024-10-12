using UnityEngine;
using TMPro;

public class StructureHealth : MonoBehaviour
{
    public float maxHealth = 100;
    private float currentHealth;
    [SerializeField] private TMP_Text healthDisplay;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    // Method to take damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage, remaining HP: " + currentHealth);
        UpdateHealthDisplay();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Update health display
    void UpdateHealthDisplay()
    {
        if (healthDisplay != null)
        {
            healthDisplay.text = currentHealth + "/" + maxHealth;
        }
    }

    // Handle death and destroy the parent object
    void Die()
    {
        Debug.Log(gameObject.name + " has died.");

        // Check if the object has a parent
        if (transform.parent != null)
        {
            Debug.Log("Destroying parent object: " + transform.parent.name);
            Destroy(transform.parent.gameObject); // Destroy the parent object
        }
        else
        {
            // If there's no parent, destroy the current object
            Destroy(gameObject);
        }
    }
}