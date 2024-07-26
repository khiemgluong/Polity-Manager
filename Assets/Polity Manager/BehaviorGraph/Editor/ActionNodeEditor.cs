using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    [CustomNodeEditor(typeof(ActionNode))]
    public class ActionNodeEditor : NodePositionEditor
    {
        public override Color GetTint()
        {
            return new Color(.5f, .5f, .5f);
        }

        public override void OnHeaderGUI()
        {
            base.OnHeaderGUI(); 
            GUIStyle centeredStyle = new(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            SerializedProperty orderProp = serializedObject.FindProperty("order");
            EditorGUILayout.LabelField("ACTION " + orderProp.intValue, centeredStyle);
        }
    }
}

