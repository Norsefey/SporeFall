using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainUI : MonoBehaviour
{
    private TrainHandler tHandler;
    public Slider HPBar;

    void Start()
    {
        HPBar.maxValue = tHandler.maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHPDisplay(float value)
    {
        HPBar.value = value;
    }
}
