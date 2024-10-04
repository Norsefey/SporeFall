using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float deathTime = 2;
    public float damage = 10;
    [SerializeField] GameObject bulletResidue;

    private void Update()
    {
        Destroy(gameObject, deathTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Instantiate(bulletResidue, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
