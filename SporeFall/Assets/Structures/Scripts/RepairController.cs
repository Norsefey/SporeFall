using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private float healAmount = 3f;        // Amount of health to restore each tick
    [SerializeField] private float healRate = 1f;
    private float HealTime;

    [Header("Heal Area")]
    [SerializeField] private float healRadius = 10;
    [SerializeField] private SphereCollider healZone;

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHP currentHP = other.GetComponent<PlayerHP>();
            if (currentHP != null && Time.time >= HealTime)
            {
                currentHP.RestoreHP(healAmount);
                HealTime = Time.time + healRate;
            }
        }
    }
    //public void Heal(float amount)
    //{
        //currentHP = Mathf.Min(currentHP + amount, maxHP);
        //Debug.Log("Player healed. Current Health: " + currentHP);
    //}
}