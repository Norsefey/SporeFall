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
                    if(PoolManager.Instance != null)
                    {
                        // Spawn Hit VFX
                        if (!PoolManager.Instance.vfxPool.TryGetValue(hitEffect, out VFXPool pool))
                        {
                            Debug.LogError($"No pool found for Effect prefab: {hitEffect.name}");
                            //return;
                        }
                        else
                        {
                            VFXPoolingBehavior vfx = pool.Get(transform.position, transform.rotation);
                            vfx.Initialize(pool);
                        }
                    }
                    else
                    {
                        Instantiate(hitEffect, transform.position, Quaternion.identity);
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
        transform.localScale = Vector3.one;
        shouldMove = false;
        // return to pool if a pool is available
        if (pool != null)
            pool.Return(this);
        else
            Destroy(gameObject);
    }
    public void MoveForward()
    {
        moveSpeed = 80;
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
