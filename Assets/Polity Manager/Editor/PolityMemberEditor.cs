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
            SerializePolityName();
        }
        void GetPolityManagerData()
        {
            if (polityManager == null) polityManager = FindObjectOfType<PolityManager>();

            if (polityManager != null && polityManager.polities != null)
            {
                polityNames = new string[polityManager.polities.Length];
                for (int i = 0; i < polityManager.polities.Length; i++)
                    polityNames[i] = polityManager.polities[i].name;

                selectedPolityIndexProp = serializedObject.FindProperty("selectedPolityIndex");
                selectedClassIndexProp = serializedObject.FindProperty("selectedClassIndex");
                selectedFactionIndexProp = serializedObject.FindProperty("selectedFactionIndex");

                serializedObject.Update();

                selectedPolityIndex = selectedPolityIndexProp.intValue;
                selectedClassIndex = selectedClassIndexProp.intValue;
                selectedFactionIndex = selectedFactionIndexProp.intValue;

                UpdateClassNames(selectedPolityIndex);

                if (selectedClassIndex > 0)
                    UpdateFactionNames(selectedPolityIndex, selectedClassIndex);

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
                    SerializePolityName();
                }
            }
        }

        void HandleClassSelection(PolityMember p)
        {
            // Check if there is more than one option (more than just "None")
            if (classNames != null && classNames.Length > 1)
            {
                EditorGUI.BeginChangeCheck();
                selectedClassIndex = EditorGUILayout.Popup("Class", selectedClassIndex, classNames);
                if (EditorGUI.EndChangeCheck())
                {
                    p.className = classNames[selectedClassIndex];
                    selectedClassIndexProp.intValue = selectedClassIndex;
                    selectedClassIndexProp.serializedObject.ApplyModifiedProperties(); // Apply changes immediately after modifying them
                    if (selectedClassIndex > 0) // Check if the selected class is not "None"
                        UpdateFactionNames(selectedPolityIndex, selectedClassIndex);
                    else
                    {
                        factionNames = new string[] { "None" };
                        selectedFactionIndex = 0;
                        p.factionName = "";  // Clear any previous faction selection
                    }
                    SerializeClassName();
                }
            }
            else if (classNames == null || classNames.Length <= 1) // If no classes or only "None"
            {
                p.className = "";  // Clear any previous class selection
                if (classNames != null && classNames.Length == 1)
                {
                    // Automatically set class name to "None" if that's the only option
                    p.className = classNames[0];
                    selectedClassIndex = 0; // Reset index to "None"
                }
            }
        }
        void HandleFactionSelection(PolityMember p)
        {
            // Check if there is more than one option (more than just "None")
            if (selectedClassIndex > 0 && factionNames != null && factionNames.Length > 1)
            {
                EditorGUI.BeginChangeCheck();
                selectedFactionIndex = EditorGUILayout.Popup("Faction", selectedFactionIndex, factionNames);
                if (EditorGUI.EndChangeCheck())
                {
                    p.factionName = factionNames[selectedFactionIndex];
                    selectedFactionIndexProp.intValue = selectedFactionIndex;
                    selectedFactionIndexProp.serializedObject.ApplyModifiedProperties();
                    SerializeFactionName();
                }
            }
            else if (selectedClassIndex > 0 && (factionNames == null || factionNames.Length <= 1))
            {
                p.factionName = "";  // Clear any previous faction selection
                if (factionNames != null && factionNames.Length == 1)
                {
                    // Automatically set faction name to "None" if that's the only option
                    p.factionName = factionNames[0];
                    selectedFactionIndex = 0; // Reset index to "None"
                }
            }
        }

        /* ------------------------- End Selection Handlers ------------------------- */

        /* -------------------------------------------------------------------------- */
        /*                          Editor Dropdown Updaters                          */
        /* -------------------------------------------------------------------------- */
        void UpdateClassNames(int polityIndex)
        {
            if (polityManager.polities[polityIndex].classes != null && polityManager.polities[polityIndex].classes.Length > 0)
            {
                // Create a new array with an extra slot for "None"
                classNames = new string[polityManager.polities[polityIndex].classes.Length + 1];
                classNames[0] = "None"; // First entry is empty
                // Fill the rest of the array with class names
                for (int i = 0; i < polityManager.polities[polityIndex].classes.Length; i++)
                    classNames[i + 1] = polityManager.polities[polityIndex].classes[i].name;

                // Only reset the selected index if it's out of bounds now
                if (selectedClassIndex >= classNames.Length || selectedClassIndex < 0)
                    selectedClassIndex = 0; // Reset to "None" if the previous selection is invalid
            }
            else
            {
                // Handle the case where there are no classes
                classNames = new string[1];
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

                if (_class.factions != null && _class.factions.Count > 0)
                {
                    factionNames = new string[_class.factions.Count + 1];
                    factionNames[0] = "None"; // First entry is empty to represent no faction selected

                    for (int i = 0; i < _class.factions.Count; i++)
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

        /* -------------------------------------------------------------------------- */
        /*                         Serialize Properties Setters                       */
        /* -------------------------------------------------------------------------- */
        void SerializePolityName()
        {
            serializedObject.Update();
            if (polityNames != null && polityNames.Length > selectedPolityIndex && selectedPolityIndex >= 0)
            {
                SerializedProperty selectedPolityName = serializedObject.FindProperty("polityName");
                selectedPolityName.stringValue = polityNames[selectedPolityIndex];
                serializedObject.ApplyModifiedProperties();
            }
        }
        void SerializeClassName()
        {
            serializedObject.Update();
            if (classNames != null && classNames.Length > selectedClassIndex && selectedClassIndex >= 0)
            {
                SerializedProperty selectedClassName = serializedObject.FindProperty("className");
                selectedClassName.stringValue = classNames[selectedClassIndex];
                serializedObject.ApplyModifiedProperties();
                Debug.Log("Serialized class: " + classNames[selectedClassIndex]);
            }
        }
        void SerializeFactionName()
        {
            serializedObject.Update();
            if (factionNames != null && factionNames.Length > selectedFactionIndex && selectedFactionIndex >= 0)
            {
                SerializedProperty selectedFactionName = serializedObject.FindProperty("factionName");
                selectedFactionName.stringValue = factionNames[selectedFactionIndex];
                serializedObject.ApplyModifiedProperties();
                Debug.Log("Serialized faction: " + factionNames[selectedFactionIndex]);
            }
        }

        /* -------------------------------------------------------------------------- */
        /*                              Inspector Drawers                             */
        /* -------------------------------------------------------------------------- */
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