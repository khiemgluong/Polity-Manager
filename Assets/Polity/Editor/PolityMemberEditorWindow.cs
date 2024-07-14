using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    public class PolityMemberEditorWindow : EditorWindow
    {
        private PolityMember polityMemberPrefab;
        private Node myNode;
        private Vector2 scrollPosition;
        private GUIStyle nodeStyle;
        public static void ShowWindow()
        {
            PolityMemberEditorWindow window = GetWindow<PolityMemberEditorWindow>("My Custom Window");
            window.minSize = new Vector2(400, 250); // Set minimum size
        }

        private void OnEnable()
        {
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            myNode = new Node(Vector2.zero, 200, 150, null); // Creating a node at position (0,0)
        }

        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Blackboard area
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1000)); // Large area for drawing

            BeginWindows();
            if (myNode != null)
            {
                myNode.Draw(nodeStyle); // Draw the node with the specified style
            }
            EndWindows();

            GUILayout.EndScrollView();
            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
            // Your window GUI code here
            // GUILayout.Label("This is a custom window!");
            // GUILayout.Label("Select a PolityMember Prefab:", EditorStyles.boldLabel);
            // polityMemberPrefab = (PolityMember)EditorGUILayout.ObjectField("PolityMember Prefab", polityMemberPrefab, typeof(PolityMember), false);
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    // Handle mouse down events
                    if (e.button == 0) // Left click
                    {
                        Debug.Log("Left Mouse Button Clicked on Blackboard");
                    }
                    break;
            }
        }

        public class Node
        {
            public Rect rect;
            public string title;
            public string content;
            public PolityMember polityMember;
            public bool isDragged;

            public Node(Vector2 position, float width, float height, PolityMember polityMember)
            {
                rect = new Rect(position.x, position.y, width, height);
                this.polityMember = polityMember;
                title = "New Node";
                content = "Node Content";
            }

            public void Draw(GUIStyle nodeStyle)
            {
                GUILayout.BeginArea(rect, nodeStyle);
                EditorGUILayout.LabelField("Title", title);
                title = EditorGUILayout.TextField(title);
                EditorGUILayout.LabelField("Content", content);
                content = EditorGUILayout.TextField(content);
                polityMember = (PolityMember)EditorGUILayout.ObjectField("Polity Member", polityMember, typeof(PolityMember), false);
                GUILayout.EndArea();
            }

            public void Drag(Vector2 delta)
            {
                rect.position += delta;
                isDragged = true;
            }
        }
    }
}