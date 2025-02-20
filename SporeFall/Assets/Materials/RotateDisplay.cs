using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateDisplay : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void SetRotation()
    {
        transform.rotation = Quaternion.Euler(0,slider.value,0);
    }
}
