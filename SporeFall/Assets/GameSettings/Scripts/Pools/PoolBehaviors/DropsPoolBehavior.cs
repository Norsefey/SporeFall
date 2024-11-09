using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropsPoolBehavior : MonoBehaviour
{
    public DropsPool pool;

    public void Initialize(DropsPool pool)
    {
        this.pool = pool;
    }

    public void ReturnObject()
    {
        pool.Return(this);
    }
}
