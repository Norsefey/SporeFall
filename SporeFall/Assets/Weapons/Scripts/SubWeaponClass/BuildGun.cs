// Ignore Spelling: buildable
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildGun : Weapon
{
    [Header("Build Gun Settings")]
    //[SerializeField] private StructuresUI sUI;
    public GameObject[] buildableStructures; // Array of objects the player can spawn
    public Structure selectedStructure; // The currently selected placed object (for moving or deleting)
    public LayerMask groundLayer; // LayerMask to identify what is "ground"
    public LayerMask structureLayer; // LayerMask for detecting objects the player can select
    public int currentBuildIndex = 0; // Current selected object to build
    public float maxBuildDistance = 100f; // Maximum distance for building
    public float structRotSpeed = 25;
    [Tooltip("Minimum percentage of the original cost that will be refunded (0-1)")]
    [Range(0, 1)]
    public float minimumRefundPercent = 0.5f;

    public bool isEditing = false;
    private bool movingStructure = false;

    [Header("Placement Validation")]
    public LayerMask structureOverlapMask; // Layer mask for checking structure overlaps
    public Color validPlacementColor = new Color(0, 1, 0, 0.25f); // Green tint for valid placement
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.25f); // Red tint for invalid placement
    private bool isValidPlacement = true;
    private Vector3 originalPosition; // Store original position for edit mode
    private Quaternion originalRotation; // Store original rotation for edit mode
    private bool showRadius = true;
    [Header("Prompt Text")]
    [SerializeField] private string buildModeText = "<color=red>Build Mode</color> \n F to Select Placed Structure" + "\n Hold Right mouse to Preview";
    [SerializeField] private string editModeText = "<color=green>Edit Mode</color> \n LC to Move \n Hold X to Destroy \n F to return";


    // Store original colors
    private class MaterialData
    {
        public Material Material;
        public Color OriginalColor;
    }
    private MaterialData[] originalMaterials;

    public void Start()
    {
        player.pUI.SwitchStructureIcon();
    }
    public override void Fire()
    {
        // Called when player presses fire button
        if (isEditing)
        {
            movingStructure = true;
        }
    }
    private void Update()
    {
        PreviewStructure();
    }
    public void OnFireReleased()
    {
        if (movingStructure)
        {
            movingStructure = false;

            // If placement is invalid, return to original position
            if (!isValidPlacement)
            {
                selectedStructure.transform.position = originalPosition;
                selectedStructure.transform.rotation = originalRotation;
                isValidPlacement = true;
                UpdatePreviewColor(true);
                player.pUI.EnablePrompt("<color=red>Invalid Placement</color> - Returned to original position");
            }
            else
            {
                // If placement is valid, restore original colors and place
                RestoreOriginalColors();
                SetStructureToOpaque();
            }
        }
    }
    private void PreviewStructure()
    {
        Ray ray = new(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, groundLayer))
        {
            if (!isEditing && selectedStructure == null)
            {
                selectedStructure = Instantiate(buildableStructures[currentBuildIndex], hit.point, Quaternion.identity).GetComponent<Structure>();
                selectedStructure.ShowRadius(showRadius);
                StoreOriginalColors(selectedStructure.GetCurrentVisual());
                SetStructureToTransparent(selectedStructure.GetCurrentVisual());
            }
            else if (selectedStructure != null)
            {
                RotateStructure();

                // Only check for overlaps if we're in edit mode and moving
                if (isEditing && movingStructure || !isEditing)
                {
                    selectedStructure.transform.position = hit.point;
                    CheckStructureOverlap();
                }

                if (player.pController.currentState == PlayerMovement.PlayerState.Aiming)
                {
                    transform.forward = player.pCamera.myCamera.transform.forward;
                }
                else
                {
                    player.pController.RotateOnFire(this.transform, player.pCamera.myCamera.transform.forward);
                }
            }
        }else if(!isEditing && selectedStructure != null)
        {
            DestroyPreview();
        }
    }
    private void StoreOriginalColors(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        originalMaterials = new MaterialData[0];

        foreach (Renderer renderer in renderers)
        {
            MaterialData[] newData = new MaterialData[originalMaterials.Length + renderer.materials.Length];
            originalMaterials.CopyTo(newData, 0);

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                newData[originalMaterials.Length + i] = new MaterialData
                {
                    Material = renderer.materials[i],
                    OriginalColor = renderer.materials[i].color
                };
            }

            originalMaterials = newData;
        }
    }
    private void RestoreOriginalColors()
    {
        if (originalMaterials != null)
        {
            foreach (var materialData in originalMaterials)
            {
                if (materialData.Material != null)
                {
                    materialData.Material.color = materialData.OriginalColor;
                }
            }
        }
    }
    private void CheckStructureOverlap()
    {
        if (selectedStructure == null) return;

        Collider[] structureColliders = selectedStructure.GetComponentsInChildren<Collider>();
        bool hasOverlap = false;

        foreach (Collider structureCollider in structureColliders)
        {
            bool wasIsTrigger = structureCollider.isTrigger;
            structureCollider.isTrigger = true;

            Bounds bounds = structureCollider.bounds;

            Collider[] overlaps = Physics.OverlapBox(
                bounds.center,
                bounds.extents,
                selectedStructure.transform.rotation,
                structureOverlapMask
            );

            structureCollider.isTrigger = wasIsTrigger;

            foreach (Collider overlap in overlaps)
            {
                // Ignore collisions with the structure itself
                if (!overlap.transform.IsChildOf(selectedStructure.transform))
                {
                    hasOverlap = true;
                    break;
                }
            }

            if (hasOverlap) break;
        }

        isValidPlacement = !hasOverlap;
        UpdatePreviewColor(isValidPlacement);
    }
    private void UpdatePreviewColor(bool isValid)
    {
        GameObject visual = selectedStructure.GetCurrentVisual();
        Renderer[] visuals = visual.GetComponentsInChildren<Renderer>();

        Color previewColor = isValid ? validPlacementColor : invalidPlacementColor;

        foreach (Renderer renderer in visuals)
        {
            foreach (var material in renderer.materials)
            {
                Color color = material.color;
                color.r = previewColor.r;
                color.g = previewColor.g;
                color.b = previewColor.b;
                color.a = previewColor.a;
                material.color = color;
            }
        }
    }
    public void PlaceStructure()
    {
        if (!isValidPlacement)
        {
            player.pUI.EnablePrompt("<color=red>Invalid Placement - Structures Overlapping</color>");
            return;
        }

        if (player.train != null)
        {
            if (selectedStructure != null && player.train.CheckEnergy(selectedStructure.GetCurrentEnergyCost()) && selectedStructure.GetCurrentMyceliaCost() <= player.Mycelia)
            {
                player.DecreaseMycelia(selectedStructure.GetCurrentMyceliaCost());
                selectedStructure.Initialize();
                selectedStructure.ToggleStructureBehavior(true);
                selectedStructure.ShowRadius(false);
                SetStructureToOpaque();
                RestoreOriginalColors(); // Restore original colors when placing

                player.train.AddStructure(selectedStructure);
                selectedStructure = null;
                originalMaterials = null; // Clear stored colors

                player.pUI.EnablePrompt(buildModeText);
            }
            else
            {
                if (selectedStructure.GetCurrentMyceliaCost() > player.Mycelia)
                {
                    player.pUI.EnablePrompt("<color=red>Need More Mycelia</color>");
                }
                else if (!player.train.CheckEnergy(selectedStructure.GetCurrentEnergyCost()))
                {
                    player.pUI.EnablePrompt("<color=red>Too many Active Structures</color>");
                }
            }
        }
        else
        {
            if (selectedStructure != null && selectedStructure.GetCurrentMyceliaCost() <= player.Mycelia)
            {
                player.DecreaseMycelia(selectedStructure.GetCurrentMyceliaCost());
                selectedStructure.Initialize();
                SetStructureToOpaque();
                RestoreOriginalColors(); // Restore original colors when placing

                selectedStructure = null;
                originalMaterials = null; // Clear stored colors

                player.pUI.EnablePrompt(buildModeText);
            }
        }
    }
    // Update DestroyPreview to clean up stored colors
    public void DestroyPreview()
    {
        if (selectedStructure != null)
        {
            Destroy(selectedStructure.gameObject);
            selectedStructure = null;
            originalMaterials = null; // Clear stored colors
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
        player.pUI.SwitchStructureIcon();
    }
    private void SetStructureToTransparent(GameObject obj)
    {
        // Set the object material to transparent for preview
        GameObject visual = selectedStructure.GetCurrentVisual();
        Renderer[] visuals = visual.GetComponentsInChildren<Renderer>();
        if (visuals != null)
        {
            foreach (Renderer renderer in visuals)
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
    }
    private void SetStructureToOpaque()
    {
        if(selectedStructure == null)
            return;

        // Set the object material to opaque for final placement
        GameObject visual = selectedStructure.GetCurrentVisual();
        Renderer[] visuals = visual.GetComponentsInChildren<Renderer>();
        if (visuals != null)
        {
            foreach (Renderer renderer in visuals)
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
    }
    // Edit Mode Functions
    public void EnterEditMode()
    {
        if(selectedStructure != null)
        {
            selectedStructure.gameObject.SetActive(false);
            Destroy(selectedStructure.gameObject); // Destroy the current preview
        }
        isEditing = true;
        selectedStructure = null;
    }
    public void ExitEditMode()
    {
        if (selectedStructure != null)
        {
            SetStructureToOpaque();
            RestoreOriginalColors();
            selectedStructure.ToggleStructureBehavior(true);
            selectedStructure.ShowRadius(false);
        }
        selectedStructure = null;
        originalMaterials = null;
        player.pUI.EnablePrompt(buildModeText);
        isEditing = false;
    }
    public bool SelectStructure()
    {
        Ray ray = new(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, structureLayer) && !movingStructure)
        {
            if (selectedStructure == null)
            {
                selectedStructure = hit.collider.transform.parent.GetComponent<Structure>();
                StoreOriginalColors(selectedStructure.GetCurrentVisual());
                SetStructureToTransparent(selectedStructure.gameObject);
                // Store original position and rotation
                originalPosition = selectedStructure.transform.position;
                originalRotation = selectedStructure.transform.rotation;
                Debug.Log("Structure selected: " + selectedStructure.name);
                player.pUI.EnablePrompt(selectedStructure.name);
                selectedStructure.ToggleStructureBehavior(false);
                selectedStructure.ShowRadius(showRadius);

                UpdatePreviewColor(true);
            }
            return true;
        }
        else if (selectedStructure != null)
        {
            RestoreOriginalColors();
            DeselectStructure();
            return false;
        }
        else
        {
            return false;
        }
    }
    private void DeselectStructure()
    {
        if (selectedStructure != null && !movingStructure)
        {
            SetStructureToOpaque();
            RestoreOriginalColors();
            selectedStructure.ToggleStructureBehavior(true);
            selectedStructure.ShowRadius(false);

            selectedStructure = null;
            originalMaterials = null;
            player.pUI.EnablePrompt(editModeText);
        }
    }
    public void RotateStructure()
    {
        if (selectedStructure != null)
        {
            float yRot = 0;
            if (player.myDevice is Gamepad)
            {
                // left and right on d-Pad on controller
                 yRot = player.pInput.rotateStructAction.ReadValue<Vector2>().x * structRotSpeed * Time.deltaTime;
            }
            else
            {
                // mouse scroll wheel uses Y
                 yRot = player.pInput.rotateStructAction.ReadValue<Vector2>().y * structRotSpeed * Time.deltaTime;
            }

            selectedStructure.transform.Rotate(new Vector3(0, yRot, 0));
        }
    }
  /*  public void UpgradeStructure()
    {

        if (selectedStructure.CanUpgrade(player.Mycelia))
        {
            player.DecreaseMycelia(selectedStructure.GetCurrentMyceliaCost());
            selectedStructure.Upgrade();
            SetStructureToTransparent(selectedStructure.GetCurrentVisual());
            if(player.train != null)
                player.train.UpdateEnergyUsage();
        }
        else
        {
            if (selectedStructure.IsMaxLevel())
            {
                player.pUI.EnablePrompt("MAX LEVEL");
            }
            else
            {
                Debug.Log("Not Enough Mycelia");
                player.pUI.EnablePrompt("Not Enough Mycelia");
            }
        }
    }*/
    public void SellStructure()
    {
        if (selectedStructure != null)
        {
            Structure toDelet = selectedStructure;
            //player.mycelia += toDelet.GetMyceliaCost();
            selectedStructure = null;
            if (player.train != null)
            {
                player.IncreaseMycelia(CalculateStructureRefund(toDelet));
                player.train.RemoveStructure(toDelet);
            }
            Debug.Log("Structure Deleted");
            player.pUI.EnablePrompt(editModeText);
        }
    }
    public void ToggleShowRadius()
    {
        showRadius = !showRadius;
        if (selectedStructure != null)
        {
            selectedStructure.ShowRadius(showRadius);
        }
    }
    private float CalculateStructureRefund(Structure structure)
    {
        StructureHP structureHP = structure.GetStructureHP();

        // Calculate HP percentage (clamped between 0 and 1)
        float healthPercentage = Mathf.Clamp01(structureHP.CurrentHP / structureHP.maxHP);
        // Calculate refund percentage, scaled between minimumRefundPercent and 1
        float refundPercentage = Mathf.Lerp(minimumRefundPercent, 1f, healthPercentage);
        // Calculate final refund amount and round to nearest integer
        int refundAmount = Mathf.RoundToInt(structure.GetCurrentMyceliaCost() * refundPercentage);

        return refundAmount;
    }
}
