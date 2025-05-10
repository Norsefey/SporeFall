using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStructure : MonoBehaviour
{
    [SerializeField] private StructureHP healthComponent;
    public bool hasStructure = false;
    private Structure myStructure;

    private void OnEnable()
    {
        healthComponent.OnHPChange += CheckDeath;
    }
    private void OnDisable()
    {
        healthComponent.OnHPChange -= CheckDeath;
    }
    private void CheckDeath(Damageable dam, float damageAmount)
    {
        if(healthComponent.CurrentHP <= 0 && myStructure != null)
        {// remove holding structure on Death
            DestroyMyStructure();
        }
    }
    public void SetStructure(Structure structure)
    {// places structure to hold
        hasStructure = true;
        myStructure = structure;
    }
    private void DestroyMyStructure()
    {
        if (myStructure != null)
        {// removes holding structure
            myStructure.myPlatform = null;
            myStructure.onPlatform = false;
            myStructure.ReturnToPool();
        }
    }
    public void RemoveStructure()
    {// Call to remove the structure it is holding
        DestroyMyStructure();
        hasStructure = false;
        myStructure = null;
    }
}
