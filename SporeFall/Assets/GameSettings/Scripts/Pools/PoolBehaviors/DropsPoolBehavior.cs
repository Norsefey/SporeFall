using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropsPoolBehavior : MonoBehaviour
{
    protected DropsPool pool;

    public void Initialize(DropsPool pool)
    {
        this.pool = pool;
    }
}
