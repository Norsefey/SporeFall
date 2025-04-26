using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentGameManager : MonoBehaviour
{
    public static PersistentGameManager Instance { get; private set; }

    [SerializeField] private Color player1Primary = Color.white;
    [SerializeField] private Color player1Secondary = Color.blue;
    [SerializeField] private Color player2Primary = Color.black;
    [SerializeField] private Color player2Secondary = Color.red;


    public float completionTime;
    private bool easyMode = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetColor(ColorPickerMode mode, Color color)
    {
        switch (mode)
        {
            case ColorPickerMode.Player1Primary:
                player1Primary = color;
                break;
            case ColorPickerMode.Player1Secondary:
                player1Secondary = color;
                break;
            case ColorPickerMode.Player2Primary:
                player2Primary = color;
                break;
            case ColorPickerMode.Player2Secondary:
                player2Secondary = color;
                break;
        }
    }
    public Color GetColor(ColorPickerMode mode)
    {
        return mode switch
        {
            ColorPickerMode.Player1Primary => player1Primary,
            ColorPickerMode.Player1Secondary => player1Secondary,
            ColorPickerMode.Player2Primary => player2Primary,
            ColorPickerMode.Player2Secondary => player2Secondary,
            _ => Color.white
        };
    }
    public bool GetEasyMode() { return easyMode;}
    public void SetEasyMode(bool toggle) { easyMode = toggle;}

    public void ResetCompletionTimer()
    {
        completionTime = 0;
    }
}
