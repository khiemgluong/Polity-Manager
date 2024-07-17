using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    [CustomEditor(typeof(PolityManager))]
    public class PolityManagerEditor : Editor
    {
        Vector2 scrollPosition;
        const float gridSize = 20, headerWidth = 120;
        public override void OnInspectorGUI()
        {
            PolityManager manager = (PolityManager)target;
            EditorGUI.BeginChangeCheck();

            SerializedProperty polities = serializedObject.FindProperty("polities");
            EditorGUILayout.PropertyField(polities, true);

            SerializedProperty playerPolity = serializedObject.FindProperty("playerPolity");
            EditorGUILayout.PropertyField(playerPolity, true);

            SerializedProperty serializedRelationships = serializedObject.FindProperty("serializedRelationships");
            EditorGUILayout.PropertyField(serializedRelationships, true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                manager.DeserializeMatrix();
                EditorUtility.SetDirty(manager);
            }

            if (manager.relationships == null || manager.relationships.GetLength(0) != manager.polities.Length || manager.relationships.GetLength(1) != manager.polities.Length)
                manager.relationships = new PolityRelation[manager.polities.Length, manager.polities.Length];
            GUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            // Create the matrix GUI with headers
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-47);
            EditorGUILayout.LabelField("", GUILayout.Width(headerWidth)); // Top-left corner blank space
            foreach (var faction in manager.polities)
            {
                GUILayout.Space(-1); // Add space before each label
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(faction.name), GUI.skin.label, GUILayout.Width(gridSize), GUILayout.Height(headerWidth));
                RotateText(labelRect, faction.name, 270);
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < manager.polities.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(manager.polities[i].name, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight }, GUILayout.Width(headerWidth));

                for (int j = 0; j < manager.polities.Length; j++)
                {
                    string tooltipText = $"{manager.polities[i].name} & {manager.polities[j].name} | {manager.relationships[i, j]}";
                    GUIContent buttonContent = new("", tooltipText);  // Tooltip text as the second parameter
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(gridSize), GUILayout.Height(gridSize));
                    if (i == j) EditorGUI.DrawRect(rect, Color.gray); // Self-relation, grayed out
                    else
                    {
                        // EditorGUI.BeginDisabledGroup(manager.DisableInteractivity);
                        if (GUI.Button(rect, buttonContent))
                        {
                            manager.relationships[i, j] = GetNextRelationship(manager.relationships[i, j]);
                            manager.relationships[j, i] = manager.relationships[i, j]; // Optional: Set reciprocal relationship
                            manager.SerializeMatrix();
                        }
                        // EditorGUI.EndDisabledGroup();

                        Color color = GetColorForRelationship(manager.relationships[i, j]);
                        EditorGUI.DrawRect(rect, color);
                        GUI.Label(rect, ""); // Optionally add labels or icons
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Save changes
            if (GUI.changed) EditorUtility.SetDirty(manager);

            if (GUILayout.Button("Polity Member Graph"))
            {
                var window = EditorWindow.GetWindow<PolityMemberGraph>("Polity Manager");
                // window.SetPolityManager(manager);
                // window.SetPolityObjects(manager.polities);
            }
        }

        void RotateText(Rect rect, string text, float angle)
        {
            Matrix4x4 matrixBackup = GUI.matrix;
            // Recalculate pivot point to be at the center bottom of the initial rectangle.
            Vector2 pivotPoint = new(rect.x + rect.height / 2, rect.y + rect.width / 2);
            GUIUtility.RotateAroundPivot(angle, pivotPoint);
            Rect adjustedRect = new(rect.x - 50, rect.y, headerWidth, gridSize);

            // EditorGUI.DrawRect(adjustedRect, new Color(0.8f, 0.8f, 0.8f, 0.5f));
            GUIStyle style = new(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            GUI.Label(adjustedRect, text, style); GUI.matrix = matrixBackup;
        }

        Color GetColorForRelationship(PolityRelation relationship)
        {
            return relationship switch
            {
                PolityRelation.Neutral => Color.yellow,
                PolityRelation.Allies => Color.green,
                PolityRelation.Enemies => Color.red,
                _ => Color.white,
            };
        }

        PolityRelation GetNextRelationship(PolityRelation current)
        {
            return current switch
            {
                PolityRelation.Neutral => PolityRelation.Enemies,
                PolityRelation.Allies => PolityRelation.Neutral,
                PolityRelation.Enemies => PolityRelation.Allies,
                _ => PolityRelation.Neutral,
            };
        }
    }
}