using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    // Opens doors as player progresses through tutorial
    [SerializeField] GameObject door;
    private bool canProgress = true;

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
        Destroy(door);
        if (canProgress == true)
        {
            Tutorial.Instance.ProgressTutorial();
            canProgress = false;
        }
    }
}
