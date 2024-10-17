using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBulletBehavior : MonoBehaviour
{
    public float deathTime = 2;
    [SerializeField] GameObject bulletResidue;
    float dmg = 15f;
    public string enemyTag = "Enemy";

    private void Update()
    {
        Destroy(gameObject, deathTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(enemyTag))
        {
            // Try to get the EnemyHealth script on the object and deal damage
            if (collision.gameObject.TryGetComponent<EnemyControls>(out var hp))
            {
                hp.TakeDamage(dmg);  // Apply 100 damage to the enemy
            }
        }
        Instantiate(bulletResidue, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
