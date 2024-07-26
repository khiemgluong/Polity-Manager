using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    [CustomNodeEditor(typeof(CompositorNode))]
    public class CompositorNodeEditor : NodePositionEditor
    {
        public override Color GetTint()
        {
            return new Color(.4f, .4f, .4f);
        }
        public override void OnHeaderGUI()
        {
            base.OnHeaderGUI(); 
            GUIStyle centeredStyle = new(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            SerializedProperty orderProp = serializedObject.FindProperty("order");
            EditorGUILayout.LabelField("COMPOSITOR " + orderProp.intValue, centeredStyle);
        }
    }
}