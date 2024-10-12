using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    [SerializeField] private GameObject structBehavior;
    [SerializeField] private GameObject[] visuals;
    [SerializeField] private float[] cost = new float[] { 25, 30, 50 };
    private int structLevel = 0;
    public int StructLevel {  get { return structLevel; } }
    private bool maxLevel = false;
    public void PurchaseStructure()
    {
        structBehavior.SetActive(true);
        GetComponent<Collider>().enabled = true; // Enable collider for the final object
        structLevel++;
    }
    public void ToggleStructureController(bool toggle)
    {
        structBehavior.SetActive(toggle);
    }
    public void UpgradeStructure()
    {
        visuals[structLevel - 1].SetActive(false);
        visuals[structLevel].SetActive(true);
        structLevel++;
        if (structLevel >= visuals.Length)
        {
            maxLevel = true;
            // so we can refer to the last visual
            structLevel -= 1;
        }
            
    }
    public bool CanUpgrade(float mycelia)
    {
       if(mycelia >= GetCost() && !maxLevel)
        {
            return true;
        }
        else
        {
            return false; 
        }
    }
    public float GetCost()
    {
        return cost[structLevel];
    }
    public bool AtMaxLevel()
    {
        return maxLevel;
    }
    public GameObject CurrentVisual()
    {
        return visuals[structLevel];
    }
}
