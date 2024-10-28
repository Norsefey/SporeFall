using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PayloadHP : Damageable
{
    [SerializeField] private TMP_Text healthDisplay;
    private Payload payload;

    // Start is called before the first frame update
    private void Start()
    {
        currentHP = maxHP; // Initialize health
        UpdateUI();
    }
    protected override void Die()
    {
        Debug.Log("Payload Destroyed");
        payload.DestroyPayload();
    }

    protected override void UpdateUI()
    {
        if(healthDisplay != null)
            healthDisplay.text = currentHP.ToString() + ":" + maxHP.ToString();
    }
    public void SetManager(Payload payload)
    {
        this.payload = payload;
    }
}
