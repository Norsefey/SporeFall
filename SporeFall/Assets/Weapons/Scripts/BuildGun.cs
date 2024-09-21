// Ignore Spelling: buildable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildGun : Weapon
{
    public GameObject[] buildableObjects; // Array of objects the player can spawn
    public float maxBuildDistance = 100f; // Maximum distance for building
    public LayerMask groundLayer; // LayerMask to identify what is "ground"
    public LayerMask objectLayer; // LayerMask for detecting objects the player can select

    public int currentBuildIndex = 0; // Current selected object to build
    private GameObject selectedObject; // The currently selected placed object (for moving or deleting)
    public override void Fire()
    {
        // Called when player presses fire button
        PreviewObject();
    }
    public void OnFireReleased()
    {
        // Called when player releases the fire button
        //PlaceObject();
        Destroy(selectedObject); // Destroy the current preview

    }
    private void PreviewObject()
    {
        Ray ray = new Ray(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);
        RaycastHit hit;

        // Check if we hit the ground layer
        if (Physics.Raycast(ray, out hit, maxBuildDistance, groundLayer))
        {
            // Create or move the preview object to the hit point on the ground
            if (selectedObject == null)
            {
                selectedObject = Instantiate(buildableObjects[currentBuildIndex], hit.point, Quaternion.identity);
                selectedObject.GetComponent<Collider>().enabled = false; // Disable collider for preview
                SetObjectToTransparent(selectedObject); // Make the object transparent to show it's a preview
            }
            else
            {
                selectedObject.transform.position = hit.point; // Update position of preview

                if (player.pController.currentState == PlayerMovement.PlayerState.Aiming)
                {
                    Debug.Log("Only Rotating Gun");
                    transform.forward = player.pCamera.myCamera.transform.forward;

                }
                else
                {
                    Debug.Log("Rotating Character");
                    player.pController.RotateOnFire(this.transform, player.pCamera.myCamera.transform.forward);
                }
            }
        }
    }
    public void PlaceObject()
    {
        if (selectedObject != null)
        {
            // Place the object in the world when the fire button is released
            selectedObject.GetComponent<Collider>().enabled = true; // Enable collider for the final object
            SetObjectToOpaque(selectedObject); // Make the object opaque
            selectedObject = null; // Clear the selected object

        }
    }
    public void CycleBuildObject()
    {
        // Cycle to the next object in the buildableObjects array
        currentBuildIndex = (currentBuildIndex + 1) % buildableObjects.Length;
        if (selectedObject != null)
        {
            Destroy(selectedObject); // Destroy the current preview
        }
        player.pUI.AmmoDisplay(this);
    }
    public string SelectedStructure()
    {
        if(selectedObject == null)
            return "Build Mode";
        else 
            return selectedObject.name;
    }
    private void SetObjectToTransparent(GameObject obj)
    {
        // Set the object material to transparent for preview
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_Surface", 1); // Set to Transparent
                material.SetFloat("_Blend", 0);
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                // Enable alpha clipping if needed (optional for transparency)
                material.SetFloat("_AlphaClip", 0);
                material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

                material.SetFloat("_ZWrite", 0); // Disable ZWrite for transparent objects

                Color color = material.color;
                color.a = 0.25f; // Half transparency
                material.color = color;
            }
        }
    }
    private void SetObjectToOpaque(GameObject obj)
    {
        // Set the object material to opaque for final placement
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_Surface", 0); // Set surface type to Opaque
                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

                // Disable blending for opaque rendering
                material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);

                // Enable ZWrite for opaque objects
                material.SetFloat("_ZWrite", 1);

                // Ensure AlphaClipping is off
                material.SetFloat("_AlphaClip", 0);

                // Set the material's keyword for opaque rendering
                material.EnableKeyword("_SURFACE_TYPE_OPAQUE");
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

                Color color = material.color;
                color.a = 1f; // Full opacity
                material.color = color;
            }
        }
    }
    public void SelectObject()
    {
        // Use raycast to detect if the player is looking at a placed object to select it for moving or destroying
        Ray ray = new Ray(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxBuildDistance, objectLayer))
        {
            selectedObject = hit.collider.gameObject; // Select the hit object
            Debug.Log("Object selected: " + selectedObject.name);
        }
    }
    public void MoveSelectedObject()
    {
        if (selectedObject != null)
        {
            // Raycast again to find new ground position to move the object
            Ray ray = new Ray(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxBuildDistance, groundLayer))
            {
                selectedObject.transform.position = hit.point; // Move the selected object to the new location
                Debug.Log("Object moved to: " + hit.point);
            }
        }
    }
    public void DestroySelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject); // Destroy the selected object
            selectedObject = null; // Clear the selection
            Debug.Log("Object destroyed");
        }
    }
}
