using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainUI : MonoBehaviour
{
    [SerializeField] private TrainHP trainHP;

    [SerializeField] private Slider HPBar;
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private Slider HPDelayBar;
    private float delayedHP;

    private void Start()
    {
        HPText.text = "Integrity: 100%";
        if(trainHP != null)
            trainHP.OnHPChange += UpdateHPDisplay;
    }

    public void SetMaxHP(float max)
    {
        HPBar.maxValue = max;
        HPBar.value = max;
        HPDelayBar.maxValue = max;
        HPDelayBar.value = max;
        delayedHP = max;
    }
    public void UpdateHPDisplay(Damageable trainHP, float value)
    {
        HPBar.value = trainHP.CurrentHP;
        float hpPercantage = ((trainHP.CurrentHP / trainHP.MaxHP) * 100);

        if (hpPercantage > 0)
        {
            HPText.text = "Integrity: " + hpPercantage.ToString("F0") + "%";
            //Debug.Log("Updating train HP bar");
        }

        else if (hpPercantage <= 0)
        {
            HPText.text = "Integrity: 0%";
            //Debug.Log("Updating train HP bar");
        }

        if (delayedHP < trainHP.CurrentHP)
        {
            //Debug.Log("DelayedHP is less than current HP");
            delayedHP = trainHP.CurrentHP;
            HPDelayBar.value = delayedHP;
            //Debug.Log("Raising delayedHP to equal current HP");
        }
        else if (delayedHP > trainHP.CurrentHP)
        {
            //Debug.Log("DelayedHP is greater than current HP");
            StartCoroutine(HPDelayCooldown());
        }
    }
    public void ChangeHPDisplay(string newText)
    {
        HPText.text = newText;
    }
    IEnumerator HPDelayCooldown()
    {
        yield return new WaitForSeconds(1f);
        while (delayedHP > trainHP.CurrentHP)
        {
            //Debug.Log("Reducing delayedHP");
            delayedHP = delayedHP - .5f;
            //Debug.Log("Train HP is: " + trainHP.CurrentHP);
            //Debug.Log("Delayed HP is: " + delayedHP);
            HPDelayBar.value = delayedHP;
            //Debug.Log("HPDelay Bar value is: " + HPDelayBar.value);
        }
        if (delayedHP < trainHP.CurrentHP)
        {
            //Debug.Log("DelayedHP has been reduced lower than current HP, raising");
            delayedHP = trainHP.CurrentHP;
            HPDelayBar.value = delayedHP;
        }
    }
}
