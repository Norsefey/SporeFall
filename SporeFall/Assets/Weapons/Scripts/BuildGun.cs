using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildGun : Weapon
{
    public GameObject[] structures;
    public int index = 0;
    Ray myRay;
    public override void Fire()
    {
        PlaceStructure();
    }

    private void PlaceStructure()
    {
        // Apply bullet spread
        Vector3 shootDirection = player.pCamera.myCamera.transform.forward;
        transform.forward = shootDirection;
        Ray ray = new Ray(player.pCamera.myCamera.transform.position, shootDirection);
        myRay = ray;
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f)) // Range of the hitscan weapon
        {
            Debug.Log(weaponName + " hit: " + hit.collider.name);
            Instantiate(structures[index], hit.point, Quaternion.identity);
            // Apply damage to the hit object, if applicable
            // hit.collider.GetComponent<Health>()?.TakeDamage(damage);
        }
    }

    public GameObject CurrentStructure()
    {
        return structures[index];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(myRay);
    }
}
