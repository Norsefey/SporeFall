using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public GameObject[] visuals;
    [SerializeField] private GameObject structBehavior;
    private int structLevel = 0;
    public int StructLevel {  get { return structLevel; } }
    public float[] cost = new float[] { 25 ,30, 50};
  
    public void PurchaseStructure()
    {
        structBehavior.SetActive(true);
        GetComponent<Collider>().enabled = true; // Enable collider for the final object
        structLevel++;
    }
    public void ToggleStructure(bool toggle)
    {
        structBehavior.SetActive(toggle);

    }

    public void UpgradeStructure()
    {
        if (structLevel < visuals.Length)
        {
            visuals[structLevel - 1 ].SetActive(false);
            visuals[structLevel].SetActive(true);
            structLevel++;
        }

    }
}
