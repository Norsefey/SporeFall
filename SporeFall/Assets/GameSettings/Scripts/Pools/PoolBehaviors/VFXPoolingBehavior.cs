using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPoolingBehavior : MonoBehaviour
{
    private VFXPool pool;
    private bool shouldMove = false;
    private Vector3 targetPos;
    private float moveSpeed;

    [SerializeField] private GameObject hitEffect;

    private void Update()
    {
        if (shouldMove)
        {
            if (targetPos == Vector3.zero)
                transform.position += transform.forward * (moveSpeed * Time.deltaTime);
            else
            {
                if(Vector3.Distance(transform.position, targetPos) < .5f)
                {
                    // Get VFX from pool
                    if (!PoolManager.Instance.vfxPool.TryGetValue(hitEffect, out VFXPool pool))
                    {
                        Debug.LogError($"No pool found for enemy prefab: {hitEffect.name}");
                        //return;
                    }
                    else
                    {
                        VFXPoolingBehavior vfx = pool.Get(transform.position, transform.rotation);
                        vfx.Initialize(pool);
                    }
                    ReturnBullet();

                }
                transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
        }
    }

    public void MoveToLocation(Vector3 target, float speed)
    {
        targetPos = target;
        moveSpeed = speed;

        shouldMove = true;
    }
    private void ReturnBullet()
    {
        targetPos = Vector3.zero;
        shouldMove = false;
        pool.Return(this);
    }
    public void MoveForward()
    {
        moveSpeed = 50;
        shouldMove = true;
        Invoke(nameof(ReturnBullet), 1f);
    }
    public void Initialize(VFXPool pool)
    {
        this.pool = pool;
    }

    private void OnParticleSystemStopped()
    {
        if(pool != null)
            pool.Return(this);
        else
            Destroy(gameObject);
    }
}
