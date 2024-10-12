// Ignore Spelling: buildable
using UnityEngine;

public class BuildGun : Weapon
{
    public GameObject[] buildableStructures; // Array of objects the player can spawn
    public float maxBuildDistance = 100f; // Maximum distance for building
    public LayerMask groundLayer; // LayerMask to identify what is "ground"
    public LayerMask structureLayer; // LayerMask for detecting objects the player can select
    private int currentBuildIndex = 0; // Current selected object to build
    private Structure selectedStructure; // The currently selected placed object (for moving or deleting)
    public float structRotSpeed = 25;

    public bool isEditing = false;
    private bool movingStructure = false;
    // structures
    public int maxStructures = 10;
    private int currentStructures = 0;
    public override void Fire()
    {
        // Called when player presses fire button
        PreviewStructure();
        if (isEditing)
            movingStructure = true;
    }
    public void OnFireReleased()
    {
        if (!isEditing)
        {
            // Called when player releases the fire button
            Destroy(selectedStructure.gameObject); // Destroy the current preview
        }
        else
        {
            movingStructure = false;
        }
    }
    private void PreviewStructure()
    {
        Ray ray = new(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);

        // Check if we hit the ground layer
        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, groundLayer))
        {
            // Create or move the preview object to the hit point on the ground
            if (!isEditing && selectedStructure == null)
            {
                //Debug.Log("Creating New Structure");
                selectedStructure = Instantiate(buildableStructures[currentBuildIndex], hit.point, Quaternion.identity).GetComponent<Structure>();
                selectedStructure.GetComponent<Collider>().enabled = false; // Disable collider for preview
                SetStructureToTransparent(selectedStructure.gameObject); // Make the object transparent to show it's a preview
            }
            else if(selectedStructure != null)
            {
                selectedStructure.transform.position = hit.point; // Update position of preview
                RotateStructure();
                if (player.pController.currentState == PlayerMovement.PlayerState.Aiming)
                {

                    //Debug.Log("Only Rotating Gun");
                    transform.forward = player.pCamera.myCamera.transform.forward;
                }
                else
                {
                    //Debug.Log("Rotating Character");
                    player.pController.RotateOnFire(this.transform, player.pCamera.myCamera.transform.forward);
                }
            }
        }
    }
    public void PlaceStructure()
    {
        if (selectedStructure != null && currentStructures < maxStructures && selectedStructure.GetCost() <= player.mycelia)
        {
            player.mycelia -= selectedStructure.GetCost();
            selectedStructure.PurchaseStructure();
            SetStructureToOpaque(selectedStructure.gameObject); // Make the object opaque
            if(player.train != null)
                selectedStructure.transform.SetParent(player.train.structureHolder, true);
            currentStructures++;
            selectedStructure = null; // Clear the selected object

            player.pUI.EnablePrompt("<color=red>Build Mode</color> \nUse Q/E to change Structure" + "\n F to Select Structure" + "\n Hold Right mouse to Preview");

        }
    }
    public void CycleSelectedStructure(float indexIncrement)
    {
        // Cycle to the next object in the buildableObjects array
        currentBuildIndex += (int)indexIncrement;
        if (currentBuildIndex >= buildableStructures.Length)
            currentBuildIndex = 0;
        else if(currentBuildIndex < 0)
            currentBuildIndex = buildableStructures.Length - 1;

        if(selectedStructure != null)
            Destroy(selectedStructure.gameObject);

        player.pUI.EnablePrompt(buildableStructures[currentBuildIndex].name);
    }
    private void SetStructureToTransparent(GameObject obj)
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
    private void SetStructureToOpaque(GameObject obj)
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

    // Edit Mode Functions
    public void EnterEditMode()
    {
        isEditing = true;
    }
    public void ExitEditMode()
    {
        DeselectStructure();
        player.pUI.EnablePrompt("<color=red>Build Mode</color> \nUse Q/E to change Structure" + "\n F to Select Structure" + "\n Hold Right mouse to Preview");
        isEditing = false;
    }
    public bool SelectStructure()
    {
        // Use raycast to detect if the player is looking at a placed object to select it for moving or destroying
        Ray ray = new(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, structureLayer) && !movingStructure)
        {
            selectedStructure = hit.collider.gameObject.GetComponent<Structure>(); // Select the hit object
            SetStructureToTransparent(selectedStructure.gameObject);
            Debug.Log("Structure selected: " + selectedStructure.name);
            player.pUI.EnablePrompt(selectedStructure.name);
            selectedStructure.ToggleStructureController(false);
            return true;
        }
        else if(movingStructure)
        {
            return false;
        }
        else
        {
            Debug.Log("No Structure");
            DeselectStructure();
            player.pUI.EnablePrompt("<color=green>Edit Mode</color> \n RC to Move \n Hold X to Destroy \n Z to Upgrade \n F to return");
            return false;
        }
    }
    protected void DeselectStructure()
    {
        if (selectedStructure != null && !movingStructure)
        {
            SetStructureToOpaque(selectedStructure.gameObject); // Make the object opaque
            selectedStructure.ToggleStructureController(true);// Enable Behavior
            selectedStructure = null;
            player.pUI.DisablePrompt();
        }
    }
    public void DestroySelectedObject()
    {
        if (selectedStructure != null)
        {
            Destroy(selectedStructure.gameObject); // Destroy the selected object
            selectedStructure = null; // Clear the selection
            isEditing = false;
        }
    }
    public void RotateStructure()
    {
        if (selectedStructure != null)
        {
            float yRot = player.pInput.rotateStructAction.ReadValue<Vector2>().y * structRotSpeed * Time.deltaTime;
            selectedStructure.transform.Rotate(new Vector3(0, yRot, 0));
        }
    }
    public void UpgradeStructure()
    {

        if (selectedStructure.CanUpgrade(player.mycelia))
        {
            player.mycelia -= selectedStructure.GetCost();
            selectedStructure.UpgradeStructure();
            SetStructureToTransparent(selectedStructure.CurrentVisual());
        }
        else
        {
            if (selectedStructure.AtMaxLevel())
            {
                player.pUI.EnablePrompt("MAX LEVEL");
            }
            else
            {
                Debug.Log("Not Enough Mycelia");
                player.pUI.EnablePrompt("Not Enough Mycelia");
            }
        }
    }
}
