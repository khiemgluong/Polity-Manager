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


        private SerializedObject serializedPolityProband;

        private SerializedProperty familyProperty;
        Vector2 scrollPosition = Vector2.zero;  // Make sure this is a class-level variable
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
        private int itemToDelete = -1; // Flag to mark index for deletion, initialized to an invalid index        private List<SerializedObject> nodes = new List<SerializedObject>();
        private List<SerializedProperty> nodeFamilyProperties = new List<SerializedProperty>();
        private List<SerializedObject> nodeGameObjects = new List<SerializedObject>();
        private List<SerializedObject> nodePolityMembers = new List<SerializedObject>();
        private List<Rect> nodeRects = new List<Rect>();
        private float nodeWidth = 250; // Reasonable width for each node
        private float nodeHeight = 350; // Reasonable height for each node
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
            float sidebarHeight = position.height * 0.5f;
            float mainPanelWidth = position.width * 0.8f;
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
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mainPanelWidth), GUILayout.Height(mainPanelHeight));

            if (GUILayout.Button("Add Node", GUILayout.Width(100)))
            {
                AddNode();
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2000)); // Adjust height as necessary for your canvas size

            BeginWindows();

            for (int i = 0; i < nodePolityMembers.Count; i++)
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
            nodeGameObjects.Add(null);
            nodePolityMembers.Add(null);
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
            if (nodePolityMembers[id] != null)
            {
                nodePolityMembers[id].Update();
                nodeGameObjects[id].Update();
            }

            bool shouldDeleteNode = false;  // Flag to track node deletion
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // The close button
            if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                shouldDeleteNode = true;
            GUILayout.EndHorizontal();
            if (shouldDeleteNode)
            {
                Debug.LogError("Removing node at index: " + id);
                // Properly handle the removal, ensuring no GUI calls are made post deletion
                nodePolityMembers.RemoveAt(id);
                nodeRects.RemoveAt(id);
                nodeFamilyProperties.RemoveAt(id);
                return;
            }
            /* -------------------------------------------------------------------------- */
            /*                          PolityMember Object Field                         */
            /* -------------------------------------------------------------------------- */
            EditorGUILayout.LabelField("Assign or Change PolityMember Prefab:");
            PolityMember member = EditorGUILayout.ObjectField("PolityMember", nodePolityMembers[id]?.targetObject as PolityMember, typeof(PolityMember), false) as PolityMember;
            // Check if a new PolityMember has been assigned or changed
            if (member != null && PrefabUtility.IsPartOfPrefabAsset(member.gameObject) && (nodePolityMembers[id] == null || member != nodePolityMembers[id].targetObject as PolityMember))
            {
                nodePolityMembers[id] = new SerializedObject(member); // PolityMember
                nodeGameObjects[id] = new SerializedObject(member.gameObject); // GameObject
                nodeFamilyProperties[id] = nodePolityMembers[id].FindProperty("family");
            }

            if (nodePolityMembers[id] != null)
            {
                SerializedObject node = nodePolityMembers[id];
                SerializedProperty familyProperty = nodeFamilyProperties[id];

                if (familyProperty != null)
                {
                    EditorGUILayout.PropertyField(familyProperty, new GUIContent("Family Object"));
                }

                if (GUILayout.Button("Save Changes to Prefab"))
                {
                    GameObject go = nodeGameObjects[id].targetObject as GameObject;
                    if (go != null)
                    {
                        SavePrefab(go);
                    }
                    else
                    {
                        Debug.LogError("GameObject is null. Cannot save prefab.");
                    }
                }

                node.ApplyModifiedProperties();
                nodeGameObjects[id].ApplyModifiedProperties();
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