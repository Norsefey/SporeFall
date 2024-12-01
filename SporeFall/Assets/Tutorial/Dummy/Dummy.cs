using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dummy : Damageable
{
    // Health properties
    [SerializeField] private TMP_Text hpDisplay;
    [Tooltip("Thing Used to go to progress tutorial")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject weaponPickUp;
    private void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }
    public override void TakeDamage(float damage)
    {
        Debug.Log("Base took damage");
        base.TakeDamage(damage);
    }
    protected override void Die()
    {
        // what happens upon death
        nextButton.SetActive(true);
        weaponPickUp.SetActive(true);
        Tutorial.Instance.ProgressTutorial();
        Destroy(transform.parent.gameObject);
    }
    protected override void UpdateUI()
    {
        if (hpDisplay != null)
        {
            hpDisplay.text = currentHP.ToString("F0") + "/" + maxHP.ToString();
        }
    }
}
