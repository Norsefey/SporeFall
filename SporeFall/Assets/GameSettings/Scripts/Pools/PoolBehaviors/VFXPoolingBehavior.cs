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
                    ToggleHitEffect(true);
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
        Invoke(nameof(ReturnBullet), 1f);
    }
    private void ToggleHitEffect(bool toggle)
    {
        if (hitEffect != null) 
        {
            hitEffect.SetActive(toggle);
        }
    }
    private void ReturnBullet()
    {
        ToggleHitEffect(false);
        targetPos = Vector3.zero;
        shouldMove = false;


        Debug.Log("Returning to pool");
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
        Debug.Log(gameObject.name + ": Initialized");
        this.pool = pool;
    }
  /*  public void OnParticleSystemStopped()
    {
        Debug.Log("Returning to pool");
        pool.Return(this);
    }*/
}
