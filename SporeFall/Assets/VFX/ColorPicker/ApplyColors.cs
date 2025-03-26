using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RendererMaterialSetup
{
    public string name;
    public SkinnedMeshRenderer renderer;
    public int primaryMaterialIndex = 0;
    public int secondaryMaterialIndex = 1;
}
public class ApplyColors : MonoBehaviour
{
    [SerializeField] private RendererMaterialSetup[] renderers;

    [SerializeField] private bool setOnAwake;
    private void Awake()
    {
        if (setOnAwake)// for when no game manager, or character is not a part of the player
            SetColors(1);
    }

    // Called on player join by game Manager
    public void SetColors(int index)
    {
        if (PlayerColorManager.Instance != null)
        {
            Color primaryColor;
            Color secondaryColor;
            if (index == 2)
            {
                primaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player2Primary);
                secondaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player2Secondary);
            }
            else
            {
                primaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player1Primary);
                secondaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player1Secondary); 
            }
           

            foreach (var setup in renderers)
            {
                if (setup.renderer != null && setup.renderer.materials.Length > Mathf.Max(setup.primaryMaterialIndex, setup.secondaryMaterialIndex))
                {
                    setup.renderer.materials[setup.primaryMaterialIndex].SetColor("_BaseColor", primaryColor);
                    setup.renderer.materials[setup.secondaryMaterialIndex].SetColor("_BaseColor", secondaryColor);
                }
            }
        }
    }
}
