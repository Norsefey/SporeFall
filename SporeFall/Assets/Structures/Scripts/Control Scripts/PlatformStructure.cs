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
        {
            myStructure.onPlatform = false;
            myStructure.myPlatform = null;
            myStructure.ReturnToPool();
        }
    }

    public void SetStructure(Structure structure)
    {
        hasStructure = true;
        myStructure = structure;
    }
    public void RemoveStructure()
    {
        hasStructure = false;
        myStructure = null;
    }
}
