using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Optional interface for objects that need initialization/cleanup when spawned/despawned
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}
public class Pool : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
