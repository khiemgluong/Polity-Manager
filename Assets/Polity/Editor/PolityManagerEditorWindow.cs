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
        private PolityObject[] polities;
        private PolityManager polityManager;
        public List<PolityMember> polityMembers;
        private SerializedObject serializedPolityManager;
        private SerializedProperty familitiesProperty;


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
        private PolityMember polityMemberPrefab;
        private SerializedObject serializedPolityMember;
        private SerializedProperty familyProperty;

        void OnGUI()
        {
            // if (polityManager != null && polityManager.polities != null)
            // {
            //     foreach (var polity in polityManager.polities)
            //     {
            //         EditorGUILayout.BeginVertical("box");
            //         EditorGUILayout.LabelField("Name:", polity.name);
            //         EditorGUILayout.LabelField("Factions:");
            //         SerializedObject serializedObject = new SerializedObject(polityManager);
            //         SerializedProperty factionsList = serializedObject.FindProperty("polities").GetArrayElementAtIndex(Array.IndexOf(polityManager.polities, polity)).FindPropertyRelative("name");
            //         EditorGUILayout.PropertyField(factionsList, true);
            //         serializedObject.ApplyModifiedProperties();
            //         EditorGUILayout.EndVertical();
            //     }
            // }
            // else
            // {
            //     EditorGUILayout.LabelField("No Polity Manager found.");
            // }

            // Calculate dimensions for the panels
            float sidebarWidth = position.width * 0.2f;
            float mainPanelWidth = position.width * 0.8f;
            float sidebarHeight = position.height * 0.5f;
            float mainPanelHeight = position.height;

            serializedPolityManager.Update(); // Prepare the serialized object for editing

            // Families sidebar with interactive list
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
            for (int i = 0; i < familitiesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty familyProp = familitiesProperty.GetArrayElementAtIndex(i);
                SerializedProperty familyNameProp = familyProp.FindPropertyRelative("name"); // Access the 'name' field
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

            // Members sidebar
            GUILayout.BeginArea(new Rect(0, sidebarHeight, sidebarWidth, sidebarHeight), "Members", GUI.skin.window);
            GUILayout.Label("Content for Members");
            GUILayout.EndArea();

            // Main panel

            GUILayout.BeginArea(new Rect(sidebarWidth, 0, mainPanelWidth, mainPanelHeight), "Main Window", GUI.skin.window);

            EditorGUILayout.LabelField("Select a PolityMember Prefab:");
            EditorGUI.BeginChangeCheck();
            polityMemberPrefab = EditorGUILayout.ObjectField("PolityMember Prefab", polityMemberPrefab, typeof(PolityMember), false) as PolityMember;

            if (EditorGUI.EndChangeCheck())
            {
                if (polityMemberPrefab != null && PrefabUtility.IsPartOfPrefabAsset(polityMemberPrefab))
                {
                    serializedPolityMember = new SerializedObject(polityMemberPrefab);
                    familyProperty = serializedPolityMember.FindProperty("family");  // ensure 'familyObject' is the exact name of the property
                }
            }

            if (polityMemberPrefab != null && serializedPolityMember != null)
            {
                serializedPolityMember.Update();  // Make sure to call Update before drawing properties

                // Display and edit properties
                if (familyProperty != null)
                {
                    EditorGUILayout.PropertyField(familyProperty, new GUIContent("Family Object"));
                }

                serializedPolityMember.ApplyModifiedProperties(); // Apply changes to serialized object

                if (GUILayout.Button("Save Changes to Prefab"))
                {
                    SavePrefab();
                }
            }
            GUILayout.EndArea();

            // Members sidebar
            GUILayout.BeginArea(new Rect(0, sidebarHeight, sidebarWidth, sidebarHeight), "Members", GUI.skin.window);
            GUILayout.Label("Content for Members");
            GUILayout.EndArea();

            // Update the serialized object if changes were made
            if (serializedPolityManager.ApplyModifiedProperties())
            {
                // Optionally notify the PolityManager that its data has changed
            }
        }

        private void SavePrefab()
        {
            if (serializedPolityMember != null)
            {
                serializedPolityMember.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(polityMemberPrefab.gameObject, AssetDatabase.GetAssetPath(polityMemberPrefab.gameObject));
                Debug.Log("Changes saved to prefab!");
            }
        }

        private void AddFamilyName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                familitiesProperty.InsertArrayElementAtIndex(familitiesProperty.arraySize);
                SerializedProperty newFamilyProp = familitiesProperty.GetArrayElementAtIndex(familitiesProperty.arraySize - 1);
                SerializedProperty nameProp = newFamilyProp.FindPropertyRelative("name");
                nameProp.stringValue = name; // Set the name of the new FamilyObject
                serializedPolityManager.ApplyModifiedProperties(); // Apply changes to ensure they're saved
                newFamilyName = ""; // Clear the input field after adding
            }
        }
        // Method to set the polity objects from outside
        public void SetPolityObjects(PolityObject[] objects)
        {
            polities = objects;
        }

        private void PerformDeletion(int index)
        {
            if (index >= 0 && index < familitiesProperty.arraySize)
            {
                familitiesProperty.DeleteArrayElementAtIndex(index);
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
            string[] familyNames = new string[familitiesProperty.arraySize];
            for (int i = 0; i < familitiesProperty.arraySize; i++)
            {
                familyNames[i] = familitiesProperty.GetArrayElementAtIndex(i).stringValue;
            }
            return familyNames;
        }

        public void SetPolityManager(PolityManager _polityManager)
        {
            polityManager = _polityManager;
            serializedPolityManager = new SerializedObject(polityManager);

            familitiesProperty = serializedPolityManager.FindProperty("families");
        }
    }
}