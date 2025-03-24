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
        if (setOnAwake)
            SetColors(1);
    }

    public void SetColors(int index)
    {
        if (PlayerColorManager.Instance != null)
        {
            Color primaryColor;
            Color secondaryColor;
            if (index == 2)
            {
                primaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player2Primary); // Adjust per player
                secondaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player2Secondary); // Adjust per player
            }
            else
            {
                primaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player1Primary); // Adjust per player
                secondaryColor = PlayerColorManager.Instance.GetColor(ColorPickerMode.Player1Secondary); // Adjust per player
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
