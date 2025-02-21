using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlayerMaterials : MonoBehaviour
{

    [SerializeField]private SkinnedMeshRenderer[] robotParts;
    [SerializeField]private Material[] altMaterials;

    public void ChangeMaterials()
    {
        foreach (var part in robotParts)
        {
            Material[] mats = part.materials;
            for (int y = 0; y < mats.Length; y++)
            {
                //Debug.Log(part.name + ": " + mats[y]);
                mats[y] = new Material(altMaterials[y]);
            }
            // apply new materials
            part.materials = mats;
        }
    }
}
