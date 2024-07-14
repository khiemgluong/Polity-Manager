using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    public class PolityManagerEditorWindow : EditorWindow
    {
        private PolityManager polityManager;
        private SerializedObject serializedPolityManager;
        private SerializedProperty familiesProperty;

        private PolityMember polityMemberPrefab;
        private PolityMember polityMemberProband;

        private SerializedObject serializedPolityMember;
        private SerializedObject serializedPolityProband;

        private SerializedProperty familyProperty;
        // public static List<string> familyNames = new List<string> { "Family 1", "Family 2", "Family 3" };
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<PolityManagerEditorWindow>("Polity Manager");
            window.minSize = new Vector2(200, 100); // Define minimum size
            var screenResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            var windowSize = screenResolution * 0.5f; // Set window size to 50% of the screen size
            var windowPosition = (screenResolution - windowSize) * 0.5f; // Center the window
            // Set the window size and position
            window.position = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
            window.Show();
        }

        private int selectedIndex = -1;  // Default to no selection
        private string selectedFamilyName = "Select a family";  // Default text
        private string newFamilyName = "";
        private int itemToDelete = -1; // Flag to mark index for deletion, initialized to an invalid index
        private List<SerializedObject> nodes = new List<SerializedObject>();
        private List<SerializedProperty> nodeFamilyProperties = new List<SerializedProperty>();
        private List<Rect> nodeRects = new List<Rect>();
        private float nodeWidth = 404; // Reasonable width for each node
        private float nodeHeight = 202; // Reasonable height for each node
        private int nodeSpacing = 10; // Spacing between nodes

        private GUIStyle nodeStyle;

        private void InitializeStyles()
        {
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f));  // Dark gray color
            backgroundTexture.Apply();
            nodeStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5),
                // Set the border as needed; adjust these values based on your needs
                border = new RectOffset(4, 4, 4, 4),
                // Use the custom background texture
                normal = { background = backgroundTexture },
            };
        }

        private List<PolityMember> polityMembers = new List<PolityMember>();


        void OnGUI()
        {

            if (nodeStyle == null)
            {
                InitializeStyles(); // Ensure styles are initialized
            }
            // Calculate dimensions for the panels
            float sidebarWidth = position.width * 0.2f;
            float mainPanelWidth = position.width * 0.8f;
            float sidebarHeight = position.height * 0.5f;
            float mainPanelHeight = position.height;

            serializedPolityManager.Update(); // Prepare the serialized object for editing

            /* -------------------------------------------------------------------------- */
            /*                                  FAMILIES                                  */
            /* -------------------------------------------------------------------------- */
            GUILayout.BeginArea(new Rect(0, 0, sidebarWidth, sidebarHeight), "Families", GUI.skin.window);
            EditorGUILayout.BeginVertical();

            // Add new family name
            EditorGUILayout.BeginHorizontal();
            newFamilyName = EditorGUILayout.TextField(newFamilyName);
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                AddFamilyName(newFamilyName);
            }
            EditorGUILayout.EndHorizontal();

            // List existing family names with a delete button and make them clickable
            for (int i = 0; i < familiesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty familyProp = familiesProperty.GetArrayElementAtIndex(i);
                SerializedProperty familyNameProp = familyProp.FindPropertyRelative("surname"); // Access the 'name' field
                string familyName = familyNameProp.stringValue; // Use stringValue to get the string value

                if (GUILayout.Button(familyName, GUILayout.ExpandWidth(true)))
                {
                    selectedIndex = i;
                    selectedFamilyName = familyName;  // Update the selected name to reflect in the main window
                }

                // Deletion button
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    itemToDelete = i; // Mark this item for deletion
                }

                EditorGUILayout.EndHorizontal();
            }
            // Handle deletion outside the loop
            if (itemToDelete != -1)
            {
                PerformDeletion(itemToDelete);
                itemToDelete = -1; // Reset deletion flag
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            /* ------------------------------ FAMILIES END ------------------------------ */

            // Members sidebar
            // GUILayout.BeginArea(new Rect(0, sidebarHeight, sidebarWidth, sidebarHeight), "Members", GUI.skin.window);
            // GUILayout.Label("Content for Members");
            // GUILayout.EndArea();

            /* -------------------------------------------------------------------------- */
            /*                                 MAIN PANEL                                 */
            /* -------------------------------------------------------------------------- */
            GUILayout.BeginArea(new Rect(sidebarWidth, 0, mainPanelWidth, mainPanelHeight), "Main Window", GUI.skin.window);
            Vector2 scrollPosition = Vector2.zero;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mainPanelWidth), GUILayout.Height(mainPanelHeight));
            EditorGUI.BeginChangeCheck();
            polityMemberProband = EditorGUILayout.ObjectField("polityMemberProband", polityMemberProband, typeof(PolityMember), false) as PolityMember;

            if (EditorGUI.EndChangeCheck())
            {
                if (polityMemberProband != null && PrefabUtility.IsPartOfPrefabAsset(polityMemberProband))
                {
                    serializedPolityProband = new SerializedObject(polityMemberProband);
                    familyProperty = serializedPolityProband.FindProperty("family");
                }
            }
            if (polityMemberProband != null && serializedPolityProband != null)
            {
                serializedPolityProband.Update();  // Make sure to call Update before drawing properties
                if (familyProperty != null)
                {
                    //Deserialize & Loop through each PolityMember of familyProperty which is FamilyObject , if any PolityMember is found, add it to a list of PolityMember called polityMembers
                    // TraverseFamily(polityMemberProband);
                }
                // serializedPolityMember.ApplyModifiedProperties(); // Apply changes to serialized object
            }


            if (GUILayout.Button("Add Node", GUILayout.Width(100)))
            {
                AddNode();
            }

            BeginWindows();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodeRects[i] = GUI.Window(i, nodeRects[i], DrawNodeWindow, "Node " + i);
            }
            EndWindows();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            /* ----------------------------- MAIN PANEL END ----------------------------- */

            // Update the serialized object if changes were made
            if (serializedPolityManager.ApplyModifiedProperties())
            {
                // Optionally notify the PolityManager that its data has changed
            }
        }

        private void AddNode()
        {
            Debug.Log("Attempting to add node...");
            // Simply add a null placeholder for new nodes
            nodes.Add(null);
            nodeFamilyProperties.Add(null);

            Rect lastRect = new Rect(10, 10, nodeWidth, nodeHeight);
            if (nodeRects.Count > 0)
            {
                lastRect = nodeRects[nodeRects.Count - 1];
                lastRect.x += nodeWidth + nodeSpacing;
            }
            nodeRects.Add(lastRect);
            Debug.Log("Node added at position: " + lastRect);
        }

        void DrawNodeWindow(int id)
        {
            // Start checking and updating the node
            if (nodes[id] != null)
            {
                nodes[id].Update();  // Update the serialized object if it exists
            }

            // Always show the ObjectField for PolityMember assignment or change
            EditorGUILayout.LabelField("Assign or Change PolityMember Prefab:");
            PolityMember member = EditorGUILayout.ObjectField("PolityMember Prefab", nodes[id]?.targetObject as PolityMember, typeof(PolityMember), false) as PolityMember;

            // Check if a new PolityMember has been assigned or changed
            if (member != null && PrefabUtility.IsPartOfPrefabAsset(member) && (nodes[id] == null || member != nodes[id].targetObject as PolityMember))
            {
                nodes[id] = new SerializedObject(member);  // Reassign the node to the new PolityMember
                nodeFamilyProperties[id] = nodes[id].FindProperty("family");  // Find and store the family property
            }

            // Work with the potentially new node data
            if (nodes[id] != null)
            {
                SerializedObject node = nodes[id];
                SerializedProperty familyProperty = nodeFamilyProperties[id];

                // Display and edit properties of the PolityMember
                if (familyProperty != null)
                {
                    EditorGUILayout.PropertyField(familyProperty, new GUIContent("Family Object"));
                }

                // Apply modifications and save functionality
                node.ApplyModifiedProperties();
                if (GUILayout.Button("Save Changes to Prefab"))
                {
                    SavePrefab(node.targetObject as GameObject);
                }

                node.ApplyModifiedProperties();  // Ensure all changes are applied
            }

            GUI.DragWindow();
        }
        private void SavePrefab(GameObject prefab)
        {
            PrefabUtility.SaveAsPrefabAsset(prefab, AssetDatabase.GetAssetPath(prefab));
            Debug.Log("Changes saved to prefab!");
        }

        private void AddFamilyName(string surname)
        {
            if (!string.IsNullOrWhiteSpace(surname))
            {
                familiesProperty.InsertArrayElementAtIndex(familiesProperty.arraySize);
                SerializedProperty newFamilyProp = familiesProperty.GetArrayElementAtIndex(familiesProperty.arraySize - 1);
                SerializedProperty nameProp = newFamilyProp.FindPropertyRelative("surname");
                nameProp.stringValue = surname; // Set the name of the new FamilyObject
                serializedPolityManager.ApplyModifiedProperties(); // Apply changes to ensure they're saved
                newFamilyName = ""; // Clear the input field after adding
            }
        }
        // Method to set the polity objects from outside
        public void SetPolityObjects(PolityObject[] objects)
        {
            // polities = objects;
        }

        private void PerformDeletion(int index)
        {
            if (index >= 0 && index < familiesProperty.arraySize)
            {
                familiesProperty.DeleteArrayElementAtIndex(index);
                if (selectedIndex == index) // Handle deletion of the selected item
                {
                    selectedIndex = -1;
                    selectedFamilyName = "Select a family";
                }
                serializedPolityManager.ApplyModifiedProperties(); // Apply changes immediately
            }
        }
        private string[] GetFamilyNamesArray()
        {
            string[] familyNames = new string[familiesProperty.arraySize];
            for (int i = 0; i < familiesProperty.arraySize; i++)
            {
                familyNames[i] = familiesProperty.GetArrayElementAtIndex(i).stringValue;
            }
            return familyNames;
        }

        public void SetPolityManager(PolityManager _polityManager)
        {
            polityManager = _polityManager;
            serializedPolityManager = new SerializedObject(polityManager);

            familiesProperty = serializedPolityManager.FindProperty("families");
        }


        private void TraverseFamily(PolityMember polityMember)
        {
            if (polityMember == null || polityMembers.Contains(polityMember))
                return;

            // Add the current proband to the list of processed PolityMembers
            polityMembers.Add(polityMember);
            Debug.Log("added member: " + polityMember.name);

            // Serialized object of the current proband
            SerializedObject serializedProband = new SerializedObject(polityMember);
            SerializedProperty familyProp = serializedProband.FindProperty("family");

            if (familyProp != null)
            {
                serializedProband.Update();

                // Access family members and recursively explore their families
                AddFamilyMember(familyProp.FindPropertyRelative("father").objectReferenceValue as PolityMember);
                AddFamilyMember(familyProp.FindPropertyRelative("mother").objectReferenceValue as PolityMember);

                // Explore partners
                SerializedProperty partnersProp = familyProp.FindPropertyRelative("partners");
                for (int i = 0; i < partnersProp.arraySize; i++)
                {
                    AddFamilyMember(partnersProp.GetArrayElementAtIndex(i).objectReferenceValue as PolityMember);
                }

                // Explore children
                SerializedProperty childrenProp = familyProp.FindPropertyRelative("children");
                for (int i = 0; i < childrenProp.arraySize; i++)
                {
                    AddFamilyMember(childrenProp.GetArrayElementAtIndex(i).objectReferenceValue as PolityMember);
                }

                serializedProband.ApplyModifiedProperties();
            }
        }

        private void AddFamilyMember(PolityMember member)
        {
            if (member != null && !polityMembers.Contains(member))
            {
                TraverseFamily(member);
            }
        }
    }


}