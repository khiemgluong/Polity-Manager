using System.Linq;
using UnityEditor;
using UnityEngine;
using static KhiemLuong.PolityManager;
namespace KhiemLuong
{
    [CustomEditor(typeof(PolityMember))]
    public class PolityMemberEditor : Editor
    {
        PolityManager polityManager;
        string[] polityNames, classNames, factionNames;
        int selectedPolityIndex;
        int selectedClassIndex;
        int selectedFactionIndex;
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
                    UpdateClassNames(selectedPolityIndex);
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
                    polityMember.polityName = polityNames[selectedPolityIndex];
                    UpdateClassNames(selectedPolityIndex);
                    // Important: Since we are changing a property that affects the display of another property,
                    // we need to update the serialized object to reflect these changes.
                    serializedObject.Update();
                }
            }

            if (classNames != null && classNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                selectedClassIndex = EditorGUILayout.Popup("Class", selectedClassIndex, classNames);
                if (EditorGUI.EndChangeCheck())
                {
                    polityMember.className = classNames[selectedClassIndex];
                    if (selectedClassIndex > 0) // Check if the selected class is not "None"
                    {
                        UpdateFactionNames(selectedPolityIndex, selectedClassIndex); // Update factions based on selected class
                    }
                    else
                    {
                        // Handle "None" selection: Clear factions and set default values
                        factionNames = new string[] { "None" };
                        selectedFactionIndex = 0;
                        polityMember.factionName = "";  // Clear any previous faction selection
                    }
                    serializedObject.Update();
                    Debug.LogError("Class index " + selectedClassIndex);
                }
            }
            else
            {
                GUILayout.Label("No classes available for this polity.");
                polityMember.className = "";  // Clear any previous class selection
            }

            // Display the faction dropdown only if the selected class is not "None"
            if (selectedClassIndex > 0 && factionNames != null && factionNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                selectedFactionIndex = EditorGUILayout.Popup("Faction", selectedFactionIndex, factionNames);
                if (EditorGUI.EndChangeCheck())
                {
                    polityMember.factionName = factionNames[selectedFactionIndex];
                }
            }
            else if (selectedClassIndex > 0)
            {
                GUILayout.Label("No factions available for this class.");
                polityMember.factionName = "";  // Clear any previous faction selection
            }

            serializedObject.ApplyModifiedProperties();


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
                polityNames[i] = polityManager.polities[i].name;
        }
        void UpdateClassNames(int polityIndex)
        {
            if (polityManager.polities[polityIndex].classes != null && polityManager.polities[polityIndex].classes.Length > 0)
            {
                // Create a new array with an extra slot for "None"
                classNames = new string[polityManager.polities[polityIndex].classes.Length + 1];
                classNames[0] = "None"; // First entry is empty

                // Fill the rest of the array with class names
                for (int i = 0; i < polityManager.polities[polityIndex].classes.Length; i++)
                {
                    classNames[i + 1] = polityManager.polities[polityIndex].classes[i].name;
                }

                // Only reset the selected index if it's out of bounds now
                if (selectedClassIndex >= classNames.Length || selectedClassIndex < 0)
                {
                    selectedClassIndex = 0; // Reset to "None" if the previous selection is now invalid
                }
            }
            else
            {
                // Handle the case where there are no classes
                classNames = new string[1]; // Just the empty entry
                classNames[0] = "None";
                selectedClassIndex = 0;
            }
        }

        void UpdateFactionNames(int polityIndex, int classIndex)
        {
            // Adjust classIndex to account for the "None" entry in the dropdown
            int adjustedClassIndex = classIndex - 1;
            // Check if the specified polity and class indexes are valid and that factions exist
            if (polityIndex >= 0 && polityIndex < polityManager.polities.Length &&
                adjustedClassIndex >= 0 && adjustedClassIndex < polityManager.polities[polityIndex].classes.Length)
            {
                Class _class = polityManager.polities[polityIndex].classes[adjustedClassIndex];

                if (_class.factions != null && _class.factions.Length > 0)
                {
                    factionNames = new string[_class.factions.Length + 1];
                    factionNames[0] = "None"; // First entry is empty to represent no faction selected

                    for (int i = 0; i < _class.factions.Length; i++)
                        factionNames[i + 1] = _class.factions[i].name;

                    selectedFactionIndex = 0;
                }
                else
                {
                    factionNames = new string[1]; // Just the empty entry if no factions are present
                    factionNames[0] = "None";
                    selectedFactionIndex = 0;
                }
            }
            else
            {
                factionNames = new string[1];
                factionNames[0] = "Invalid selection";
                selectedFactionIndex = 0;
            }
        }

        bool InteractiveFoldout(bool foldout, string content)
        {
            Rect rect = GUILayoutUtility.GetRect(16f, 22f, new GUIStyle { fontStyle = FontStyle.Bold });
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                foldout = !foldout;
                Event.current.Use(); // Mark the event as used so it doesn't propagate further
            }
            EditorGUI.Foldout(rect, foldout, content, true);
            return foldout;
        }
        void DrawReadOnlyPolityMembersList(SerializedProperty listProperty)
        {
            // Ensure the property is not null and is a list
            if (listProperty != null && listProperty.isArray)
            {
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