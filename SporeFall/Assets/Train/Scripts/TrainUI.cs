using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainUI : MonoBehaviour
{
    private TrainHandler tHandler;
    [SerializeField] private Slider HPBar;

    void Start()
    {
        HPBar.maxValue = tHandler.maxHP;
        Debug.Log("Train HP bar set to 100");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHPDisplay(float value)
    {
        HPBar.value = value;
        Debug.Log("Updating train HP bar");
    }
}
