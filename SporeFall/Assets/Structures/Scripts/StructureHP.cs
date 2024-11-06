using UnityEngine;
using TMPro;

public class StructureHP : Damageable
{
    [SerializeField] private TMP_Text healthDisplay;

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    // Handle death and destroy the parent object
    protected override void Die()
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

    protected override void UpdateUI()
    {
        if (healthDisplay != null)
        {
            healthDisplay.text = currentHP.ToString("F0") + "/" + maxHP;
        }
    }
}