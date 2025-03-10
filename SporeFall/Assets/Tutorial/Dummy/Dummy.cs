using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dummy : Damageable
{
    // Health properties
    [SerializeField] private TMP_Text hpDisplay;
    [Tooltip("Thing Used to go to progress tutorial")]
    //[SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject weaponPickUp;

    private void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }
    public override void TakeDamage(float damage)
    {
        Debug.Log("Base took damage");
        base.TakeDamage(damage);

        UpdateHPUI();
    }
    protected override void Die()
    {
        // what happens upon death
        //nextButton.SetActive(true);
        if (Tutorial.Instance.tutorialPrompt == 5)
        {
            weaponPickUp.SetActive(true);
        }
        Tutorial.Instance.ProgressTutorial();
        Debug.Log("Dying Now");
        Destroy(transform.parent.gameObject);
    }
    protected void UpdateHPUI()
    {
        if (hpDisplay != null)
        {
            hpDisplay.text = currentHP.ToString("F0") + "/" + maxHP.ToString();
        }
    }
}
