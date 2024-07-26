using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    [CustomNodeEditor(typeof(ConditionalNode))]
    public class ConditionalNodeEditor : NodePositionEditor
    {
        public override Color GetTint()
        {
            return new Color(.3f, .3f, .3f);
        }

        public override void OnHeaderGUI()
        {
            base.OnHeaderGUI(); 
            GUIStyle centeredStyle = new(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            SerializedProperty orderProp = serializedObject.FindProperty("order");
            EditorGUILayout.LabelField("CONDITIONAL " + orderProp.intValue, centeredStyle);
        }
    }
}