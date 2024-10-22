using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainUI : MonoBehaviour
{
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

    public void UpdateHPTextDisplay(float value)
    {
        HPText.text = "Train HP: " + value + "%";
    }
    public void UpdateHPDisplay(float value)
    {
        HPBar.value = value;
        Debug.Log("Updating train HP bar");
    }
}
