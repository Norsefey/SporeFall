using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedSettings : MonoBehaviour
{

    //Handles settings that should be kept between scenes, like camera sensitivity

    public static SavedSettings instance;

    public float mouseCamSensitivity;
    public float gamepadHorCamSensitivity;
    public float gamepadVertCamSensitivity;

    private void Awake()
    {
        // start of new code
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // end of new code

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
