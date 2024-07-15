using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    public class PolityManagerEditorWindow : EditorWindow
    {
        public enum ConnectionPoint
        {
            Top,
            Right,
            Left,
            Bottom
        }
        public struct NodeConnection
        {
            public int NodeId;
            public ConnectionPoint Point;

            public NodeConnection(int nodeId, ConnectionPoint point)
            {
                NodeId = nodeId;
                Point = point;
            }
        }
        List<Rect> nodes = new List<Rect>();
        List<NodeConnection> windowsToAttach = new List<NodeConnection>();
        List<NodeConnection> attachedWindows = new List<NodeConnection>();
        float panX = 0;
        float panY = 0;
        public PolityMember rootPolityMember;
        List<PolityMember> polityMembers = new List<PolityMember>();
        public SerializedObject rootPolityMemberSerializedObj;
        Vector2 nodeSize = new Vector2(150, 150);
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<PolityManagerEditorWindow>("Polity Manager");
            window.minSize = new Vector2(200, 100); // Define minimum size
            var screenResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            var windowSize = screenResolution * 0.5f; // Set window size to 50% of the screen size
            var windowPosition = (screenResolution - windowSize) * 0.5f; // Center the window
            // Set the window size and position
            window.position = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
            window.Show();
        }

        void OnEnable()
        {
            Debug.LogError("Enabled");
            Vector2 windowCenter = new Vector2(position.width / 2, position.height / 2);
            Rect nodeRect = new Rect(windowCenter.x - nodeSize.x / 2, windowCenter.y - nodeSize.y / 2, nodeSize.x, nodeSize.y);
            // nodes.Add(new Rect(nodeRect.position, nodeSize.x, nodeSize.y)); // Assuming Node constructor takes a Rect or position and size

            // Add a new node with the calculated rectangle
            // nodes.Add(new Node(nodeRect.position, nodeSize.x, nodeSize.y)); // Assuming Node constructor takes a Rect or position and size
            nodes.Add(nodeRect);
        }
        void OnGUI()
        {
            if (windowsToAttach.Count == 2)
            {
                attachedWindows.Add(windowsToAttach[0]);
                attachedWindows.Add(windowsToAttach[1]);
                windowsToAttach.Clear();
            }

            if (attachedWindows.Count >= 2)
            {
                for (int i = 0; i < attachedWindows.Count; i += 2)
                {
                    DrawNodeCurve(nodes[attachedWindows[i].NodeId], nodes[attachedWindows[i + 1].NodeId], attachedWindows[i].Point, attachedWindows[i + 1].Point);

                }
            }

            GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));
            BeginWindows();

            if (GUILayout.Button("Create Node"))
            {
                nodes.Add(new Rect(10, 10, nodeSize.x, nodeSize.y));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Window " + i);
            }

            EndWindows();
            GUI.EndGroup();

            if (GUI.RepeatButton(new Rect(15, 5, 20, 20), "^"))
            {
                panY -= 1;
                Repaint();
            }

            if (GUI.RepeatButton(new Rect(5, 25, 20, 20), "<"))
            {
                panX -= 1;
                Repaint();
            }

            if (GUI.RepeatButton(new Rect(25, 25, 20, 20), ">"))
            {
                panX += 1;
                Repaint();
            }

            if (GUI.RepeatButton(new Rect(15, 45, 20, 20), "v"))
            {
                panY += 1;
                Repaint();
            }
        }
        void DrawNodeWindow(int id)
        {
            // Ensure that there is a placeholder for new nodes if the index exceeds the list count
            while (polityMembers.Count <= id)
            {
                polityMembers.Add(null);  // Add null to ensure the list size is appropriate
            }

            polityMembers[id] = EditorGUILayout.ObjectField("", polityMembers[id], typeof(PolityMember), false) as PolityMember;
            if (EditorGUI.EndChangeCheck())
            {
                if (polityMembers[id] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[id]))
                {
                    // rootPolityMemberSerializedObj = new SerializedObject(rootPolityMember);
                }
            }
            if (id == 0)
            {
                if (GUILayout.Button("Parents"))
                {
                    windowsToAttach.Add(new NodeConnection(id, ConnectionPoint.Top));
                }
                if (GUILayout.Button("Partners"))
                {
                    windowsToAttach.Add(new NodeConnection(id, ConnectionPoint.Right));
                }
                if (GUILayout.Button("Children"))
                {
                    windowsToAttach.Add(new NodeConnection(id, ConnectionPoint.Bottom));
                }
            }
            else
            {
                if (GUILayout.Button("Attach"))
                {
                    windowsToAttach.Add(new NodeConnection(id, ConnectionPoint.Left));
                }
            }
            GUI.DragWindow();
        }

        void DrawNodeCurve(Rect start, Rect end, ConnectionPoint startConnection, ConnectionPoint endConnection)
        {
            Vector2 startPercentage = GetPercentageFromConnectionPoint(startConnection);
            Vector2 endPercentage = GetPercentageFromConnectionPoint(endConnection);
            Color lineColor = GetStartConnectionLineColor(startConnection);
            DrawNodeCurve(start, end, startPercentage, endPercentage, lineColor);
        }

        Vector2 GetPercentageFromConnectionPoint(ConnectionPoint point)
        {
            switch (point)
            {
                case ConnectionPoint.Top: return new Vector2(0.5f, 0f);
                case ConnectionPoint.Right: return new Vector2(1.0f, 0.5f);
                case ConnectionPoint.Left: return new Vector2(0.0f, 0.5f);
                case ConnectionPoint.Bottom: return new Vector2(0.5f, 1f);
                default: return new Vector2(0.5f, 0.5f); // Default to center if unknown for some reason
            }
        }

        Color GetStartConnectionLineColor(ConnectionPoint startConnection)
        {
            switch (startConnection)
            {
                case ConnectionPoint.Top: return Color.blue;
                case ConnectionPoint.Right: return Color.green;
                case ConnectionPoint.Left: return Color.green;
                case ConnectionPoint.Bottom: return Color.red;
                default: return Color.black; // Default to center if unknown for some reason
            }
        }

        void DrawNodeCurve(Rect start, Rect end, Vector2 vStartPercentage, Vector2 vEndPercentage, Color lineColor)
        {
            Vector3 startPos = new Vector3(start.x + start.width * vStartPercentage.x, start.y + start.height * vStartPercentage.y, 0);
            Vector3 endPos = new Vector3(end.x + end.width * vEndPercentage.x, end.y + end.height * vEndPercentage.y, 0);
            Vector3 startTan = startPos + Vector3.right * (-50 + 100 * vStartPercentage.x) + Vector3.up * (-50 + 100 * vStartPercentage.y);
            Vector3 endTan = endPos + Vector3.right * (-50 + 100 * vEndPercentage.x) + Vector3.up * (-50 + 100 * vEndPercentage.y);
            Color shadowCol = new Color(200, 200, 200, 0.4f);
            for (int i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, lineColor, null, 2);
        }

    }
}