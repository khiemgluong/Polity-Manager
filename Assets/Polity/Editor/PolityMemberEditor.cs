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
        SerializedProperty selectedPolityIndexProp;
        SerializedProperty selectedClassIndexProp;
        SerializedProperty selectedFactionIndexProp;
        bool hasPolityManagerBeenFound = false; // Ensure this variable is correctly utilized

        void OnEnable()
        {
            GetPolityManagerData();
        }

        void GetPolityManagerData()
        {
            if (polityManager == null)
            {
                polityManager = FindObjectOfType<PolityManager>();
            }

            if (polityManager != null && polityManager.polities != null)
            {
                InitializePolityNames();
                selectedPolityIndexProp = serializedObject.FindProperty("selectedPolityIndex");
                selectedClassIndexProp = serializedObject.FindProperty("selectedClassIndex");
                selectedFactionIndexProp = serializedObject.FindProperty("selectedFactionIndex");

                // Ensure the serialized properties are up-to-date
                serializedObject.Update();

                selectedPolityIndex = selectedPolityIndexProp.intValue;
                selectedClassIndex = selectedClassIndexProp.intValue;
                selectedFactionIndex = selectedFactionIndexProp.intValue;

                // Update class names based on the initially selected polity index
                UpdateClassNames(selectedPolityIndex);

                // If there is a valid class index selected, update faction names
                if (selectedClassIndex > 0)
                {
                    UpdateFactionNames(selectedPolityIndex, selectedClassIndex);
                }

                serializedObject.ApplyModifiedProperties();
                hasPolityManagerBeenFound = true;
            }
        }
        public override void OnInspectorGUI()
        {
            PolityMember p = (PolityMember)target;

            SerializedProperty selectedPolityName = serializedObject.FindProperty("polityName");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(selectedPolityName, true);
            EditorGUI.EndDisabledGroup();

            serializedObject.Update();

            selectedPolityIndex = selectedPolityIndexProp.intValue;
            selectedClassIndex = selectedClassIndexProp.intValue;
            selectedFactionIndex = selectedFactionIndexProp.intValue;

            HandlePolitySelection(p);
            HandleClassSelection(p);
            HandleFactionSelection(p);

            SerializedProperty parentsSerializedProp = serializedObject.FindProperty("parents");
            SerializedProperty partnersSerializedProp = serializedObject.FindProperty("partners");
            SerializedProperty childrenSerializedProp = serializedObject.FindProperty("children");

            DrawReadOnlyPolityMembersList(parentsSerializedProp);
            DrawReadOnlyPolityMembersList(partnersSerializedProp);
            DrawReadOnlyPolityMembersList(childrenSerializedProp);

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(target);
        }

        /* -------------------------------------------------------------------------- */
        /*                             Selection Handlers                             */
        /* -------------------------------------------------------------------------- */
        void HandlePolitySelection(PolityMember p)
        {
            if (hasPolityManagerBeenFound && polityNames != null)
            {
                EditorGUI.BeginChangeCheck();
                selectedPolityIndex = EditorGUILayout.Popup("Polity", selectedPolityIndex, polityNames);
                if (EditorGUI.EndChangeCheck())
                {
                    p.polityName = polityNames[selectedPolityIndex];
                    selectedPolityIndexProp.intValue = selectedPolityIndex;
                    selectedPolityIndexProp.serializedObject.ApplyModifiedProperties();
                    UpdateClassNames(selectedPolityIndex);
                }
            }
        }

        void HandleClassSelection(PolityMember p)
        {
            if (classNames != null && classNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                selectedClassIndex = EditorGUILayout.Popup("Class", selectedClassIndex, classNames);
                if (EditorGUI.EndChangeCheck())
                {
                    p.className = classNames[selectedClassIndex];
                    selectedClassIndexProp.intValue = selectedClassIndex;
                    selectedClassIndexProp.serializedObject.ApplyModifiedProperties();
                    if (selectedClassIndex > 0) // Check if the selected class is not "None"
                        UpdateFactionNames(selectedPolityIndex, selectedClassIndex);
                    else
                    {
                        factionNames = new string[] { "None" };
                        selectedFactionIndex = 0;
                        p.factionName = "";  // Clear any previous faction selection
                    }
                    serializedObject.Update();
                }
            }
            else
            {
                GUILayout.Label("No classes available for this polity.");
                p.className = "";  // Clear any previous class selection
            }
        }

        void HandleFactionSelection(PolityMember p)
        {
            if (selectedClassIndex > 0 && factionNames != null && factionNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                selectedFactionIndex = EditorGUILayout.Popup("Faction", selectedFactionIndex, factionNames);
                if (EditorGUI.EndChangeCheck())
                {
                    p.factionName = factionNames[selectedFactionIndex];
                    selectedFactionIndexProp.intValue = selectedFactionIndex;
                    selectedFactionIndexProp.serializedObject.ApplyModifiedProperties(); // Apply changes immediately after modifying them
                }
            }
            else if (selectedClassIndex > 0)
            {
                GUILayout.Label("No factions available for this class.");
                p.factionName = "";  // Clear any previous faction selection
            }
        }
        /* ------------------------- End Selection Handlers ------------------------- */

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