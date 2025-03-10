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

    private void Start()
    {
        HPText.text = "Train HP: 100%";

        trainHP.OnHPChange += UpdateHPDisplay;
    }

    public void SetMaxHP(float max)
    {
        HPBar.maxValue = max;
        HPBar.value = max;
    }
    public void UpdateHPDisplay(Damageable trainHP, float value)
    {
        HPBar.value = trainHP.CurrentHP;
        float hpPercantage = ((trainHP.CurrentHP / trainHP.MaxHP) * 100);

        if (hpPercantage > 0)
        {
            HPText.text = "Train HP: " + hpPercantage.ToString("F0") + "%";
            Debug.Log("Updating train HP bar");
        }

        else if (hpPercantage <= 0)
        {
            HPText.text = "Train HP: 0%";
            Debug.Log("Updating train HP bar");
        }
    }
}
