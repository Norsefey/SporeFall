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
    }

    public void SetMaxHP(float max)
    {
        HPBar.maxValue = max;
    }
    public void UpdateHPDisplay(float value)
    {
        HPBar.value = value;
        float hpPercantage = ((value / trainHP.MaxHP) * 100);

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
