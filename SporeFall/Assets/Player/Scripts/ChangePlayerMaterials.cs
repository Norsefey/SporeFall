using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlayerMaterials : MonoBehaviour
{

    [SerializeField]private SkinnedMeshRenderer[] robotParts;
    [SerializeField]private MeshRenderer[] gunMesh;
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
        ChangePistolMaterials();
    }

    private void ChangePistolMaterials()
    {
        foreach (var part in gunMesh)
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
