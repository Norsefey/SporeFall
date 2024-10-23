using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    private TrainHandler train;
    [SerializeField] private GameObject structBehavior;
    [SerializeField] private GameObject[] visuals;
    //public Sprite structureSprite;
    [SerializeField] private float[] cost = new float[] { 25, 30, 50 };
    [SerializeField] private float[] energyCost = new float[] {5, 10, 15};
    private int structLevel = 0;
    public int StructLevel {  get { return structLevel; } }
    private bool maxLevel = false;
    public void PurchaseStructure()
    {
        structBehavior.SetActive(true);
        GetComponent<Collider>().enabled = true; // Enable collider for the final object
    }
    public void ToggleStructureController(bool toggle)
    {
        structBehavior.SetActive(toggle);
    }
    public void UpgradeStructure()
    {
        visuals[structLevel].SetActive(false);
        structLevel++;
        visuals[structLevel].SetActive(true);
        if (structLevel >= visuals.Length - 1)
        {
            maxLevel = true;
        }
            
    }
    public bool CanUpgrade(float mycelia)
    {
       if(mycelia >= GetMyceliaCost() && !maxLevel)
        {
            return true;
        }
        else
        {
            return false; 
        }
    }
    public float GetMyceliaCost()
    {
        return cost[structLevel];
    }
    public float GetEnergyCost()
    {
        return energyCost[structLevel];
    }
    public bool AtMaxLevel()
    {
        return maxLevel;
    }
    public GameObject CurrentVisual()
    {
        return visuals[structLevel];
    }
    public void SetTrainHandler(TrainHandler train)
    {
        this.train = train; 
    }
    private void OnDestroy()
    {
        if (train != null)
            train.RemoveStructure(this);
    }
}
