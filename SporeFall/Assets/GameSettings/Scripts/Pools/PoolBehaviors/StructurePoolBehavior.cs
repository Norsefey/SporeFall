using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePoolBehavior : MonoBehaviour
{
    public StructurePool pool;
    public void Initialize(StructurePool pool)
    {
        this.pool = pool;
    }

    public void ReturnObject()
    {
        pool.Return(this);
    }
}
