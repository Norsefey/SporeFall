using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Codice.Client.BaseCommands;

namespace SetaReceiveGISwitcher
{
    public class ReceiveGISwitcher : EditorWindow
    {

        enum SearchOption //options for searching prefabs based on GI settings.
        {
            Lightmaps,
            LightProbes
        }

        enum GlobalIlluminationOption //defines GI option to be applied to selected prefabs
        {
            Lightmaps,
            LightProbes
        }

        private SearchOption searchOption;
        private GlobalIlluminationOption selectedOption;
        private List<GameObject> foundObjects = new List<GameObject>(); //list to hold found prefabs
        private List<bool> selectedObjects = new List<bool>(); //list to track selection state of each prefab
        private List<int> indicesToRemove = new List<int>(); // list to store indices of removed objects
        private Vector2 scrollPosition; //variable for managing scroll position in the GUI
        private string nameFilter = ""; // field for filtering prefabs by name


        [MenuItem("Tools/Seta/Receive GI Switcher")] //creates editor window menu item for GI Switcher
        public static void ShowWindow()
        {
            GetWindow<ReceiveGISwitcher>("Receive GI Switcher"); //opens the tool window
        }


        void OnGUI() //OnGUI method to draw UI elements
        {
            GUILayout.Label("Global Illumination Settings", EditorStyles.boldLabel);  //display title label
            searchOption = (SearchOption)EditorGUILayout.EnumPopup("Search Prefabs Using:", searchOption);  //search option

            if (GUILayout.Button("Find Prefabs in Scene")) //button to initiate the process
            {
                FindAllPrefabsWithMeshRendererAndGI();  //call method to find all prefabs in the scene
            }
            GUILayout.Label($"Found Prefabs: {foundObjects.Count}", EditorStyles.boldLabel);  //display count of found prefabs

            if (foundObjects.Count > 0) //if there are prefabs found, display them in a scrollable list
            {
                GUILayout.Space(10); //add space for readability
                GUILayout.Label("Search and Select by Name:", EditorStyles.boldLabel); //label for the filter section
                GUILayout.BeginHorizontal(); //begin horizontal layout for filter input and button
                nameFilter = EditorGUILayout.TextField(nameFilter); //text field

                if (GUILayout.Button("Search & Select", GUILayout.Width(120))) //button
                {
                    FilterByName(); //filltering method to select only matching prefabs
                }
                GUILayout.EndHorizontal(); //fnd horizontal layout

                GUILayout.Space(10);  //add space for readability
                GUILayout.Label("Found Prefab Objects:", EditorStyles.boldLabel);   //label for list of prefabs
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));  //begin scroll view

                List<int> indicesToRemove = new List<int>();  //list to store indices of items to remove

                for (int i = 0; i < foundObjects.Count; i++) //iterate over found prefabs and display them with checkboxes and delete buttons
                {
                    GUILayout.BeginHorizontal();  //begin horizontal layout for each prefab item
                    selectedObjects[i] = GUILayout.Toggle(selectedObjects[i], GUIContent.none, GUILayout.Width(20)); //add checkbox for selection
                    EditorGUILayout.ObjectField(foundObjects[i], typeof(GameObject), true); //display prefab object in the field

                    if (GUILayout.Button("X", GUILayout.Width(25))) //button to delete prefab from the list
                    {
                        indicesToRemove.Add(i); //mark the index for removal
                    }

                    GUILayout.EndHorizontal(); //end horizontal layout
                }

                GUILayout.EndScrollView();  //end the scroll view

                foreach (var index in indicesToRemove)   //remove marked objects after iteration (so the list isn't modified while iterating)
                {
                    foundObjects.RemoveAt(index);  //remove prefab from list
                    selectedObjects.RemoveAt(index);  //remove corresponding selection state
                }

                GUILayout.Space(10);  //add space for readability
                selectedOption = (GlobalIlluminationOption)EditorGUILayout.EnumPopup("Set Receive GI to:", selectedOption);  //popup for selecting GI option
                GUILayout.BeginVertical();  //begin vertical layout for buttons

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUI.backgroundColor = Color.yellow;  //set background color to yellow for button

                if (GUILayout.Button("Apply GI to Selected Prefabs")) //button to apply GI setting
                {
                    ApplyGIToSelectedPrefabs();  //call method to apply GI to the selected prefabs
                }

                GUI.backgroundColor = Color.red;  //set background color to red for button

                if (GUILayout.Button("Apply GI to All Found Prefabs")) //button to apply GI setting
                {
                    ApplyGIToAllPrefabs();  //call method to apply GI to all prefabs
                }

                GUILayout.EndVertical();  //end vertical layout
                GUI.backgroundColor = Color.white;  //reset background color to white
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);  //separator line
            }

            else
            {
                GUILayout.Space(10); //add space if no prefabs are found
                GUILayout.Label("No prefabs matching the criteria found in the scene.", EditorStyles.helpBox); //display a message if no prefabs are found
            }
        }




        void FindAllPrefabsWithMeshRendererAndGI() //method to find all prefabs with MeshRenderer and specific GI settings in the scene
        {
            foundObjects.Clear();  //clear previous found objects list
            selectedObjects.Clear();  //clear previous selection states

            GameObject[] allGameObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None); //get all GameObjects in the scene

            foreach (GameObject obj in allGameObjects) //iterate through all objects in the scene
            {

                if (PrefabUtility.IsPartOfAnyPrefab(obj))  //check if object is part of any prefab
                {
                    MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(); //get all MeshRenderer components in object children
                    bool matchesCriteria = false;  //flag to check if object matches search criteria

                    foreach (MeshRenderer renderer in renderers)
                    {
                        if (GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, StaticEditorFlags.ContributeGI)) //check if object has Contribute GI static flag set
                        {

                            if (searchOption == SearchOption.Lightmaps && renderer.receiveGI == ReceiveGI.Lightmaps) //if search option is lightmaps and Gi type matches
                            {
                                matchesCriteria = true;  //set flag to true
                                break;
                            }

                            else if (searchOption == SearchOption.LightProbes && renderer.receiveGI == ReceiveGI.LightProbes) //if search option is LightProbes and GI type matches
                            {
                                matchesCriteria = true;  //set flag to true
                                break;
                            }
                        }
                    }

                    if (matchesCriteria) //if object matches criteria, add it to the found list
                    {
                        foundObjects.Add(obj);  //add prefab to the found objects list
                        selectedObjects.Add(false);  //add a default "not selected" state for object
                    }
                }
            }
        }



        void FilterByName() //method to filter prefabs by name
        {
            if (string.IsNullOrEmpty(nameFilter)) return; //skip if filter is empty

            for (int i = 0; i < foundObjects.Count; i++) // loop through all found prefabs
            {
                if (foundObjects[i].name.ToLower().Contains(nameFilter.ToLower())) // check if prefab name contains the typed string
                {
                    selectedObjects[i] = true; //select prefab if name matches
                }
                else
                {
                    selectedObjects[i] = false; // deselect prefab if name does not match
                }
            }
        }



        void ApplyGIToSelectedPrefabs() //method to apply selected GI setting to only selected prefabs
        {
            for (int i = 0; i < foundObjects.Count; i++)
            {
                if (selectedObjects[i])  //if prefab is selected
                {
                    ApplyGISettingToPrefab(foundObjects[i]);  //apply GI setting to this prefab
                }
            }

            FindAllPrefabsWithMeshRendererAndGI(); //reload list after click "Apply"
        }




        void ApplyGIToAllPrefabs() //method to apply selected GI setting to all found prefabs
        {

            for (int i = 0; i < foundObjects.Count; i++) // iterating through all the found prefabs
            {

                if (!indicesToRemove.Contains(i)) // check if the prefab index is not in the removal list
                {
                    ApplyGISettingToPrefab(foundObjects[i]);  // apply GI setting to this prefab
                }
            }

            FindAllPrefabsWithMeshRendererAndGI(); //reload list after click "Apply"
        }



        void ApplyGISettingToPrefab(GameObject prefab) //method to apply the selected GI setting to a single prefab
        {
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(); //get all MeshRenderer components in prefab's children

            foreach (MeshRenderer renderer in renderers) //iterate over each MeshRenderer component
            {
                if (selectedOption == GlobalIlluminationOption.Lightmaps) //apply selected GI setting to the renderer
                {
                    renderer.receiveGI = ReceiveGI.Lightmaps;  //set to Lightmaps
                }
                else if (selectedOption == GlobalIlluminationOption.LightProbes)
                {
                    renderer.receiveGI = ReceiveGI.LightProbes;  //set to LightProbes
                }

                EditorUtility.SetDirty(renderer);  //mark renderer as dirty to ensure changes are saved
            }
        }
    }
}
