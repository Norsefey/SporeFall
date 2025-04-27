// Ignore Spelling: buildable
using System.Collections.Generic;
using UnityEngine;

public class BuildGun : Weapon
{
    public List<GameObject> buildableStructures = new();
    [Header("Build Gun Settings")]
    public Structure selectedStructure; // The currently selected object (for placing, moving or deleting)
    public LayerMask placeableLayerMask;
    public LayerMask structureLayer; // LayerMask for detecting objects the player can select
    public int currentBuildIndex = 0; // Current selected object to build
    public float maxBuildDistance = 100f; // Maximum distance for building
    public float structRotSpeed = 25;
    [Tooltip("Minimum percentage of the original cost that will be refunded (0-1)")]
    [Range(0, 1)]
    public float minimumRefundPercent = 0.5f;

    public bool isEditing = false;
    public bool movingStructure = false;

    [Header("Placement Validation")]
    public LayerMask structureOverlapMask;
    public Color validPlacementColor = new(0, 1, 0, 0.25f); // Green tint for valid placement
    public Color invalidPlacementColor = new(1, 0, 0, 0.25f); // Red tint for invalid placement
    private bool isValidPlacement = true;
    private Vector3 originalPosition; // Store original position for edit mode, in case move doesn't work, returns to original position
    private Quaternion originalRotation; // Store original rotation for edit mode
    private bool showRadius = true; // player can toggle the radius display
    [Header("Prompt Text")]
    [SerializeField] private string buildModeText = "<color=red>Build Mode</color> \n Mousewheel to change Structure \n Hold Right mouse to Preview \n F to enter Edit Mode";
    [SerializeField] private string editModeText = "<color=green>Edit Mode</color> \n Left mouse to Move \n Hold X to Destroy \n F to return";

    private PlatformStructure selectedPlatform;
    private bool placingOnPlatform = false;

    private void Awake()
    {
        buildableStructures = GameManager.Instance.availableStructures;
    }

    // To Store original colors for changing preview colors
    private class MaterialData
    {
        public Material Material;
        public Color OriginalColor;
    }
    private MaterialData[] originalMaterials;
    public override void Fire()
    {
        // Called when player presses fire button
        if (isEditing)
        {
            if (selectedStructure == null)
            {
                return;
            }
            if (selectedStructure.CompareTag("Platform"))
            {
                if (selectedStructure.transform.GetChild(0).GetComponentInChildren<PlatformStructure>().hasStructure)
                {
                    player.pUI.EnablePrompt("<color=red>Holding A Structure</color> - Cannot Move");
                }
                else
                {
                    if (selectedStructure.onPlatform)
                    {
                        selectedStructure.myPlatform.RemoveStructure();
                    }

                    movingStructure = true;
                }
            }
            else
            {
                movingStructure = true;
            }
            PreviewStructure();
        }
    }
    private void Update()
    {
        if (!isEditing)
            PreviewStructure();
        else if(!movingStructure)
            SelectStructure();
    }
    public void OnFireReleased()
    {
        if (movingStructure)
        {
            movingStructure = false;

            // If placement is invalid, return to original position
            if (!isValidPlacement && selectedStructure != null)
            {
                selectedStructure.transform.SetPositionAndRotation(originalPosition, originalRotation);
                isValidPlacement = true;
                UpdatePreviewColor(true);
                player.pUI.EnablePrompt("<color=red>Invalid Placement</color> - Returned to original position");
            }
            else
            {
                if (placingOnPlatform)
                {
                    selectedPlatform.SetStructure(selectedStructure);
                    selectedStructure.myPlatform = selectedPlatform;
                    selectedStructure.onPlatform = true;
                }
                else if(selectedStructure.onPlatform)
                {
                    selectedStructure.myPlatform.RemoveStructure();
                    selectedStructure.onPlatform = false;
                }

                // If placement is valid, restore original colors and place
                RestoreOriginalColors();
                //SetStructureToOpaque();
            }
        }
    }
    public void CycleBuildableStructure(float indexIncrement)
    {
        // Cycle to the next object in the buildableObjects array
        currentBuildIndex += (int)indexIncrement;
        if (currentBuildIndex >= buildableStructures.Count)
            currentBuildIndex = 0;
        else if (currentBuildIndex < 0)
            currentBuildIndex = buildableStructures.Count - 1;

        placeableLayerMask = buildableStructures[currentBuildIndex].GetComponent<Structure>().placeableLayer;
        structureOverlapMask = buildableStructures[currentBuildIndex].GetComponent<Structure>().collisionOverlapLayer;

        if (selectedStructure != null)
        {
            RemovePreview();
            //selectedStructure.poolBehavior.ReturnObject();
        }

        selectedStructure = null;

        PreviewStructure();

        //player.pUI.SwitchStructureIcon();
    }
    private void PreviewStructure()
    {

        Ray ray = new(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, placeableLayerMask))
        {
            if (!isEditing && selectedStructure == null)
            {// spawn in a new structure
                // structures are part of the pooling system, pull from there
                if(PoolManager.Instance != null)
                {
                    if (!PoolManager.Instance.structurePool.TryGetValue(buildableStructures[currentBuildIndex], out StructurePool pool))
                    {
                        Debug.LogError($"No pool found for Structure prefab: {buildableStructures[currentBuildIndex].name}");
                        return;
                    }
                    selectedStructure = pool.Get(hit.point, Quaternion.identity).GetComponent<Structure>();
                    selectedStructure.poolBehavior.Initialize(pool);
                    selectedStructure.DisableStructureControls();
                }
                else
                {
                    selectedStructure = Instantiate(buildableStructures[currentBuildIndex], hit.point, Quaternion.identity).GetComponent<Structure>();
                    selectedStructure.DisableStructureControls();
                }


                selectedStructure.ShowRadius(showRadius);
                StoreOriginalColors(selectedStructure.GetCurrentVisual());
                //SetStructureToTransparent(selectedStructure.GetCurrentVisual());
                player.pUI.EnablePrompt(selectedStructure.GetStructureName() + "\n Cost: " + selectedStructure.GetCurrentMyceliaCost() + "\n" + selectedStructure.GetStructureDescription());

            }
            else if (selectedStructure != null)
            {// allow player to move
                // Only check for overlaps when we're in edit mode and moving, or just previewing
                if (isEditing && movingStructure || !isEditing)
                {
                    if (hit.collider.CompareTag("Platform") && !selectedStructure.CompareTag("Platform"))
                    {
                        selectedPlatform = hit.transform.GetComponent<PlatformStructure>();
                        if (selectedPlatform.hasStructure)
                        {
                            isValidPlacement = false;
                            UpdatePreviewColor(isValidPlacement);
                        }
                        else
                        {
                            selectedStructure.transform.position = hit.transform.position + (Vector3.up);
                            placingOnPlatform = true;
                            isValidPlacement = true;
                            UpdatePreviewColor(isValidPlacement);
                        }
                    }
                    else
                    {
                        placingOnPlatform = false;
                        selectedStructure.transform.position = hit.point;
                        CheckStructureOverlap();
                    }
                }

                if (player.pController.currentState == PlayerMovement.PlayerState.Aiming)
                {
                    transform.forward = player.pCamera.myCamera.transform.forward;
                }
                else
                {// player character is always looking in direction of structure
                    player.pController.RotateOnFire();
                }
            }
        }else if(!isEditing && selectedStructure != null)
        {// player not aiming at a valide placement area, remove Preview
            RemovePreview();
        }

        player.pUI.SwitchStructureIcon();
    }
    public void PlaceStructure()
    {
        if (!isValidPlacement)
        {
            //Debug.Log("Invalid Placement");
            player.pUI.EnablePrompt("<color=red>Invalid Placement - Structures Overlapping</color>");
            return;
        }
        // some test scenes do not have a train to reference off of, so check if a train is valid to do an energy check
        if (GameManager.Instance != null && GameManager.Instance.trainHandler != null && selectedStructure != null)
        {
            if (GameManager.Instance.trainHandler.CheckEnergy(selectedStructure.GetCurrentEnergyCost()) && selectedStructure.GetCurrentMyceliaCost() <= GameManager.Instance.Mycelia)
            {
                GameManager.Instance.DecreaseMycelia(selectedStructure.GetCurrentMyceliaCost());
                selectedStructure.Initialize();
                selectedStructure.ToggleStructureBehavior(true);
                selectedStructure.ShowRadius(false);

                if (placingOnPlatform)
                {
                    selectedPlatform.SetStructure(selectedStructure);
                    selectedStructure.myPlatform = selectedPlatform;
                    selectedStructure.onPlatform = true;
                }
                else
                {
                    if (selectedStructure.onPlatform)
                    {
                        selectedStructure.myPlatform.RemoveStructure();
                        selectedStructure.onPlatform = false;
                        selectedStructure.myPlatform = null;
                    }
                }

                //SetStructureToOpaque();

                RestoreOriginalColors(); // Restore original colors when placing
                // for moving train stores all active structures
                GameManager.Instance.trainHandler.AddStructure(selectedStructure);
                selectedStructure = null;
                // show build controls
                player.pUI.EnableControls(buildModeText);
                
            }
            else if(selectedStructure != null)
            {
                // tell player why placement failed, either not enough money, or too many active structure
                if (selectedStructure.GetCurrentMyceliaCost() > GameManager.Instance.Mycelia)
                {
                    player.pUI.EnablePrompt("<color=red>Need More Mycelia</color>");
                }
                else if (!GameManager.Instance.trainHandler.CheckEnergy(selectedStructure.GetCurrentEnergyCost()))
                {
                    player.pUI.EnablePrompt("<color=red>Too many Active Structures</color>");
                }
            }
        }
        else if(selectedStructure != null)
        {
            Debug.Log("Train less Placement");

            // mostly for testing the cost and placement of structures
            if (selectedStructure != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DecreaseMycelia(selectedStructure.GetCurrentEnergyCost());
                }
                selectedStructure.Initialize();
                selectedStructure.ToggleStructureBehavior(true);
                selectedStructure.ShowRadius(false);
                RestoreOriginalColors(); // Restore original colors when placing
                selectedStructure = null;
                player.pUI.EnableControls(buildModeText);

            }
        }
    }
    public void RemovePreview()
    {
        if (selectedStructure != null)
        {
            if (PoolManager.Instance != null)
            {
                selectedStructure.DisableStructureControls();
                selectedStructure.poolBehavior.ReturnObject();
            }
            else
                Destroy(selectedStructure.gameObject);

            DeselectStructure();
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
    #region Color Management
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
                    materialData.Material.SetColor("_BaseColor", materialData.OriginalColor);
                }

            }

            originalMaterials = null; // Clear stored colors
        }
        else
        {
            Debug.Log("No Original Materials");
        }
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
                material.SetColor("_BaseColor", previewColor);
            }
        }
    }
   /* private void SetStructureToTransparent(GameObject obj)
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
        if (selectedStructure == null)
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
    }*/
    #endregion

    public void SelectStructureHotKey(int index)
    {
        if (index >= buildableStructures.Count || index < 0)
            return;
        currentBuildIndex = index;
        placeableLayerMask = buildableStructures[currentBuildIndex].GetComponent<Structure>().placeableLayer;
        structureOverlapMask = buildableStructures[currentBuildIndex].GetComponent<Structure>().collisionOverlapLayer;

        if (selectedStructure != null)
        {
            RemovePreview();
        }
        selectedStructure = null;
        PreviewStructure();
    }

    #region EditMode
    public void EnterEditMode()
    {
        if(selectedStructure != null)
        {
            RemovePreview();
        }
        isEditing = true;
    }
    public void ExitEditMode()
    {
        if (selectedStructure != null)
        {
            //SetStructureToOpaque();
            RestoreOriginalColors();
            selectedStructure.ToggleStructureBehavior(true);
            selectedStructure.ShowRadius(false);
        }
        selectedStructure = null;
        originalMaterials = null;
        player.pUI.EnableControls(buildModeText);
        isEditing = false;
    }
    public bool SelectStructure()
    {
        Ray ray = new(player.pCamera.myCamera.transform.position, player.pCamera.myCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, structureLayer) && !movingStructure)
        {// we are looking at a structure select it and deactivate its control script to edit it
            //Debug.Log(hit.collider.name);
            if (selectedStructure == null)// to prevent assigning the same structure
            {
                selectedStructure = hit.collider.transform.GetComponent<Structure>();
                StoreOriginalColors(selectedStructure.GetCurrentVisual());
                //SetStructureToTransparent(selectedStructure.gameObject);
                // Store original position and rotation
                originalPosition = selectedStructure.transform.position;
                originalRotation = selectedStructure.transform.rotation;

                 player.pUI.EnablePrompt(selectedStructure.GetStructureName());

                selectedStructure.ToggleStructureBehavior(false);
                selectedStructure.ShowRadius(showRadius);

                UpdatePreviewColor(true);
            }
            return true;
        }
        else if (selectedStructure != null)
        {// we are no longer looking a structure, return it to normal
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
    {// restores selected structures to default functionality
        if (selectedStructure != null && !movingStructure)
        {
            //SetStructureToOpaque();
            RestoreOriginalColors();
            if (isEditing)
            {
                selectedStructure.ToggleStructureBehavior(true);
                player.pUI.EnableControls(editModeText);
            }


            selectedStructure.ShowRadius(false);

            selectedStructure = null;
            originalMaterials = null;
       
        }
    }
    public void RotateStructure(float direction)
    {
        if (selectedStructure != null)
        {
            selectedStructure.transform.Rotate(new Vector3(0, direction * 25, 0));
        }
    }
    public void SellStructure()
    {
        if (selectedStructure != null)
        {
            Structure toDelet = selectedStructure;
            if (PoolManager.Instance != null)
                selectedStructure.ReturnToPool();
            else
                Destroy(selectedStructure.gameObject);

            RestoreOriginalColors();
            selectedStructure.ShowRadius(false);

            selectedStructure = null;
            originalMaterials = null;

            player.pUI.EnableControls(editModeText);


            if (GameManager.Instance.trainHandler != null)
            {
                GameManager.Instance.IncreaseMycelia(toDelet.CalculateStructureRefund(minimumRefundPercent));
                GameManager.Instance.trainHandler.RemoveStructure(toDelet);
            }
            Debug.Log("Structure Sold");
            player.pUI.EnableControls(editModeText);
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
    #endregion

}
