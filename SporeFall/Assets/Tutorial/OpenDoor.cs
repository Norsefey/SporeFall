using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    // Opens doors as player progresses through tutorial
    private bool canProgress = true;
    [SerializeField] int doorNumber;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canProgress == true)
        {
            Tutorial.Instance.DestroyDoor(doorNumber);
            canProgress = false;
        }
    }
}
