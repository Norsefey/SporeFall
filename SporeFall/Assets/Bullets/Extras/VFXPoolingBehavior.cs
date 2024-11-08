using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPoolingBehavior : MonoBehaviour
{
    private VFXPool pool;

    public void Initialize(VFXPool pool)
    {
        this.pool = pool;
    }
    public void OnParticleSystemStopped()
    {
        Debug.Log("Returning to pool");
        pool.Return(this);
    }
}
