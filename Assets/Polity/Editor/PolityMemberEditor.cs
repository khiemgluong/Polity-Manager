using System.Linq;
using UnityEditor;
using UnityEngine;
namespace KhiemLuong
{
    [CustomEditor(typeof(PolityMember))]
    public class PolityMemberEditor : Editor
    {
        PolityManager polityManager;
        string[] polityNames, factionNames;
        int selectedPolityIndex = 0;
        int selectedFactionIndex = 0;
        bool hasPolityManagerBeenFound = false; // Ensure this variable is correctly utilized

        void OnEnable()
        {
            // Attempt to load PolityManager on enable if not already found
            GetPolityManagerData();
        }

        void GetPolityManagerData()
        {
            if (!hasPolityManagerBeenFound)
            {
                polityManager = FindObjectOfType<PolityManager>();
                if (polityManager != null && polityManager.polities != null)
                {
                    InitializePolityNames();
                    PolityMember polityMember = (PolityMember)target;
                    polityMember.polityName = polityNames[selectedPolityIndex];
                    UpdateFactionNames(selectedPolityIndex);
                    if (polityManager != null)
                    {
                        
                    }
                    hasPolityManagerBeenFound = true;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            PolityMember polityMember = (PolityMember)target;

            SerializedProperty selectedPolityName = serializedObject.FindProperty("polityName");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(selectedPolityName, true);
            EditorGUI.EndDisabledGroup();

            if (hasPolityManagerBeenFound && polityNames != null)
            {
                EditorGUI.BeginChangeCheck();
                selectedPolityIndex = EditorGUILayout.Popup("Polity", selectedPolityIndex, polityNames);
                if (EditorGUI.EndChangeCheck())
                {
                    // Update the polity name based on the new selection
                    polityMember.polityName = polityNames[selectedPolityIndex];
                    UpdateFactionNames(selectedPolityIndex);

                    // Important: Since we are changing a property that affects the display of another property,
                    // we need to update the serialized object to reflect these changes.
                    serializedObject.Update();
                }

                // Ensure that the polityName has been successfully updated and is not null
                if (!string.IsNullOrEmpty(polityMember.polityName))
                {
                    SerializedProperty factionLeader = serializedObject.FindProperty("polityLeader");
                    EditorGUILayout.PropertyField(factionLeader, new GUIContent("Polity Leader?"));
                }
                serializedObject.ApplyModifiedProperties();  // Ensure all changes made through the inspector are applied
            }

            // Only show the faction dropdown if there are factions to select
            if (factionNames != null && factionNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                selectedFactionIndex = EditorGUILayout.Popup("Faction", selectedFactionIndex, factionNames);
                if (EditorGUI.EndChangeCheck())
                {
                    polityMember.factionName = factionNames[selectedFactionIndex];
                }
            }
            else
            {
                GUILayout.Label("No factions available for this polity.");
                polityMember.factionName = "";  // Clear any previous faction selection
            }


            SerializedProperty parentsSerializedProp = serializedObject.FindProperty("parents");
            SerializedProperty partnersSerializedProp = serializedObject.FindProperty("partners");
            SerializedProperty childrenSerializedProp = serializedObject.FindProperty("children");

            DrawReadOnlyPolityMembersList(parentsSerializedProp);
            DrawReadOnlyPolityMembersList(partnersSerializedProp);
            DrawReadOnlyPolityMembersList(childrenSerializedProp);

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(target);
        }

        void InitializePolityNames()
        {
            polityNames = new string[polityManager.polities.Length];
            for (int i = 0; i < polityManager.polities.Length; i++)
            {
                polityNames[i] = polityManager.polities[i].name;
            }
        }

        void UpdateFactionNames(int polityIndex)
        {
            if (polityManager.polities[polityIndex].factions != null)
            {
                // Increase the size by 1 to include an empty slot at the start
                factionNames = new string[polityManager.polities[polityIndex].factions.Length + 1];
                factionNames[0] = "None"; // First entry is empty

                // Fill the rest of the array
                for (int i = 0; i < polityManager.polities[polityIndex].factions.Length; i++)
                {
                    factionNames[i + 1] = polityManager.polities[polityIndex].factions[i].name;
                }

                // If selectedFactionIndex is 0, it will automatically point to the empty string entry
                selectedFactionIndex = 0; // Reset the faction index on polity change
            }
            else
            {
                factionNames = new string[1]; // Just the empty entry if no factions are present
                factionNames[0] = "";
                selectedFactionIndex = 0;
            }
        }

        bool InteractiveFoldout(bool foldout, string content)
        {
            // Get a rectangle in the layout for the foldout
            Rect rect = GUILayoutUtility.GetRect(16f, 22f, new GUIStyle { fontStyle = FontStyle.Bold }); // You might need to adjust the style here
                                                                                                         // Use a bold style and increase the height a bit to match typical foldouts

            // Check for mouse events in the rect to toggle foldout
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                foldout = !foldout;
                Event.current.Use(); // Mark the event as used so it doesn't propagate further
            }

            // Draw the foldout
            EditorGUI.Foldout(rect, foldout, content, true);

            return foldout;
        }
        void DrawReadOnlyPolityMembersList(SerializedProperty listProperty)
        {
            // Ensure the property is not null and is a list
            if (listProperty != null && listProperty.isArray)
            {
                // Only show the foldout if there are elements in the list
                if (listProperty.arraySize > 0)
                {
                    listProperty.isExpanded = InteractiveFoldout(listProperty.isExpanded, listProperty.displayName);
                    if (listProperty.isExpanded)
                    {
                        EditorGUI.indentLevel++;

                        // Temporarily disable GUI to make the properties read-only
                        GUI.enabled = false;
                        for (int i = 0; i < listProperty.arraySize; i++)
                        {
                            SerializedProperty item = listProperty.GetArrayElementAtIndex(i);
                            EditorGUILayout.PropertyField(item, new GUIContent("Element " + i));
                        }
                        GUI.enabled = true; // Re-enable GUI after drawing the properties

                        EditorGUI.indentLevel--;
                    }
                }
            }
        }
    }
}