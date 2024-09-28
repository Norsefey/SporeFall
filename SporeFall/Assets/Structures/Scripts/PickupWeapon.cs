using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PickupWeapon : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private float rotSpeed = 45;
    [SerializeField] private float moveSpeed = 5;

    Vector3 startPos = Vector3.zero;
    private void Start()
    {
        startPos = weaponPrefab.transform.position;
    }
    private void LateUpdate()
    {
        weaponPrefab.transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
        float movement = Mathf.PingPong(Time.time * moveSpeed, .3f);
        weaponPrefab.transform.position = startPos + new Vector3(0, movement, 0);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerManager>().EnablePickUpWeapon(weaponPrefab);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerManager>().DisablePickUpWeapon();
        }
    }

}
