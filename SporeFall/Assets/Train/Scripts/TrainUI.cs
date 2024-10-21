using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainUI : MonoBehaviour
{
    [SerializeField] private Slider HPBar;

    public void SetMaxHP(float max)
    {
        HPBar.maxValue = max;
    }
    public void UpdateHPDisplay(float value)
    {
        HPBar.value = value;
        Debug.Log("Updating train HP bar");
    }
}
