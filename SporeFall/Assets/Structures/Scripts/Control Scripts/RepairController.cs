using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairController : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]
    public float healAmount, healRate, healRadius;
    [Header("Heal Area")]
    private float HealTime;


    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Healing Player");
            PlayerHP playerHP = other.GetComponent<PlayerHP>();
            if (playerHP != null && playerHP.CurrentHP < playerHP.MaxHP && Time.time >= HealTime)
            {
                playerHP.RestoreHP(healAmount);
                HealTime = Time.time + healRate;
            }
        }
    }
}