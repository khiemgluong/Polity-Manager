using System;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    using static PolityManager;
    [CustomEditor(typeof(PolityManager))]
    public class PolityManagerEditor : Editor
    {
        Vector2 scrollPosition;
        const float gridSize = 20, headerWidth = 120;

        public override void OnInspectorGUI()
        {
            PolityManager manager = (PolityManager)target;
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            SerializedProperty polities = serializedObject.FindProperty("polities");
            EditorGUILayout.PropertyField(polities, true);
            EditorGUI.EndDisabledGroup();

            SerializedProperty dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            EditorGUILayout.PropertyField(dontDestroyOnLoad, true);

            /* -------------------------------------------------------------------------- */
            /*                           POLITY RELATION MATRIX                           */
            /* -------------------------------------------------------------------------- */
            /* ----------------------------- BEGIN VERTICAL ----------------------------- */
            GUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            // Create the matrix GUI with headers
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-47);
            EditorGUILayout.LabelField("", GUILayout.Width(headerWidth));
            for (int j = manager.polities.Length - 1; j >= 0; j--)
            {
                GUILayout.Space(-1);
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(manager.polities[j].name), GUI.skin.label, GUILayout.Width(gridSize), GUILayout.Height(headerWidth));
                RotateText(labelRect, manager.polities[j].name, 270);
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < manager.polities.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(manager.polities[i].name, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight }, GUILayout.Width(headerWidth));
                // Create a grid but only for entries above the diagonal
                for (int j = manager.polities.Length - 1; j > i; j--) // Note the condition and the decrement
                {
                    string tooltipText = $"{manager.polities[i].name} & {manager.polities[j].name} | {manager.PolityRelationMatrix[i, j]}";
                    GUIContent buttonContent = new GUIContent("", tooltipText);  // Tooltip text as the second parameter
                    Rect gridRect = EditorGUILayout.GetControlRect(GUILayout.Width(gridSize), GUILayout.Height(gridSize));

                    if (GUI.Button(gridRect, buttonContent))
                    {
                        switch (Event.current.button)
                        {
                            case 0: // Left mouse button
                                manager.PolityRelationMatrix[i, j] = GetNextRelationship(manager.PolityRelationMatrix[i, j]); break;
                            case 1: // Right mouse button
                                manager.PolityRelationMatrix[i, j] = GetBackRelationship(manager.PolityRelationMatrix[i, j]); break;
                            default: return;
                        }
                        manager.PolityRelationMatrix[j, i] = manager.PolityRelationMatrix[i, j];//Set reciprocal
                        if (Application.isPlaying) OnRelationChange?.Invoke();
                    }

                    Color color = GetColorForRelationship(manager.PolityRelationMatrix[i, j]);
                    EditorGUI.DrawRect(gridRect, color);
                    GUI.Label(gridRect, ""); // Optionally add labels or icons
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView(); GUILayout.EndVertical();
            /* ------------------------------ END VERTICAL ------------------------------ */
            /* -------------------------------------------------------------------------- */
            /*                         END POLITY RELATION MATRIX                         */
            /* -------------------------------------------------------------------------- */

            if (!Application.isPlaying) if (GUILayout.Button("Member Family Graph"))
                    EditorWindow.GetWindow<PolityMemberGraph>("Polity Manager");

            // Save changes
            if (GUI.changed)
            {
                if (!Application.isPlaying)
                    manager.SerializePolityRelationMatrix();
                serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(manager);
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
                PolityRelation.Neutral => PolityRelation.Allies,
                PolityRelation.Allies => PolityRelation.Enemies,
                PolityRelation.Enemies => PolityRelation.Neutral,
                _ => PolityRelation.Neutral,
            };
        }
        PolityRelation GetBackRelationship(PolityRelation current)
        {
            return current switch
            {
                PolityRelation.Neutral => PolityRelation.Enemies,
                PolityRelation.Enemies => PolityRelation.Allies,
                PolityRelation.Allies => PolityRelation.Neutral,
                _ => PolityRelation.Neutral,
            };
        }
    }
}