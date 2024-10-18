using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anime;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private Transform handPosition;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        weaponSlot.position = handPosition.position;
    }
}
