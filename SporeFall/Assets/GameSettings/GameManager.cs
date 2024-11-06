using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public WaveManager WaveManager;
    public TrainHandler TrainHandler;


    private void Awake()
    {
        Instance = this;
    }
}
