using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    public class PolityManagerEditorWindow : EditorWindow
    {
        enum NodePoint
        {
            Top,
            Right,
            Left,
            Bottom
        }
        enum MemberRelationType
        {
            None,
            Parent,
            Partner,
            Children,
            Friends,
        }
        struct Node
        {
            public int NodeId;
            public NodePoint Point;
            public Node(int nodeId, NodePoint point)
            {
                NodeId = nodeId;
                Point = point;
            }
        }
        List<Rect> nodes = new();
        List<PolityMember> polityMembers = new();
        SerializedObject rootPolityMemberSerializedObj;
        SerializedProperty parentsSerializedProperty;
        Vector2 nodeSize = new(150, 150);

        /* ------------------------------ PAN CONTROLS ------------------------------ */
        float panX = 0, panY = 0;
        bool isDragging = false;
        Vector2 initialMousePosition;
        Vector2 dragStartPosition;

        void OnEnable()
        {
            Debug.LogError("Enabled");
            Vector2 windowCenter = new(position.width / 2, position.height / 2);
            Rect nodeRect = new(windowCenter.x - nodeSize.x / 2, windowCenter.y - nodeSize.y / 2, nodeSize.x, nodeSize.y);
            nodes.Add(nodeRect);
        }
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

        void OnGUI()
        {
            float sidebarWidth = position.width * 0.1f;
            float mainPanelWidth = position.width * 0.9f;
            float sidebarHeight = position.height * 0.5f;
            float mainPanelHeight = position.height;

            /* -------------------------------------------------------------------------- */
            /*                                  SIDEBAR                                   */
            /* -------------------------------------------------------------------------- */
            GUILayout.BeginArea(new Rect(0, 0, sidebarWidth, sidebarHeight), "Sidebar", GUI.skin.window);
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Create Node"))
            {
                nodes.Add(new Rect(10, 10, nodeSize.x, nodeSize.y));
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            /* ------------------------------- SIDEBAR END ------------------------------ */


            /* -------------------------------------------------------------------------- */
            /*                                 MAIN PANEL                                 */
            /* -------------------------------------------------------------------------- */
            GUILayout.BeginArea(new Rect(sidebarWidth, 0, mainPanelWidth, mainPanelHeight), "Main Window", GUI.skin.window);

            Rect groupRect = new(panX, panY, 100000, 100000);
            GUI.BeginGroup(groupRect);
            BeginWindows();
            // if (attachedWindows.Count >= 2)
            // {
            //     for (int i = 0; i < attachedWindows.Count; i += 2)
            //     {
            //         DrawNodeCurve(nodes[attachedWindows[i].NodeId], nodes[attachedWindows[i + 1].NodeId], attachedWindows[i].Point, attachedWindows[i + 1].Point);
            //     }
            // }
            foreach (var pair in linkedNodes)
            {
                DrawNodeCurve(nodes[pair.Key.NodeId], nodes[pair.Value.NodeId], pair.Key.Point, pair.Value.Point);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Window " + i);
            }

            EndWindows();
            GUI.EndGroup();
            GUILayout.EndArea();
            /* ----------------------------- MAIN PANEL END ----------------------------- */

            /* ------------------------------ PAN CONTROLS ------------------------------ */
            if (Event.current.type == EventType.MouseDown && groupRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
                initialMousePosition = Event.current.mousePosition;
                dragStartPosition = new Vector2(panX, panY);
                Event.current.Use();  // Consume the event so no other GUI elements use it
            }

            if (isDragging)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 currentMousePosition = Event.current.mousePosition;
                    Vector2 delta = currentMousePosition - initialMousePosition;

                    // Apply the delta to pan values
                    panX = dragStartPosition.x + delta.x;
                    panY = dragStartPosition.y + delta.y;
                    Repaint();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    isDragging = false;
                }
            }
        }
        void DrawNodeWindow(int id)
        {
            // Ensure that there is a placeholder for new nodes if the index exceeds the list count
            while (polityMembers.Count <= id)
            {
                polityMembers.Add(null);  // Add null to ensure the list size is appropriate
            }
            EditorGUI.BeginChangeCheck();
            polityMembers[id] = EditorGUILayout.ObjectField("", polityMembers[id], typeof(PolityMember), false) as PolityMember;

            if (id == 0)
            {
                if (EditorGUI.EndChangeCheck())
                {
                    if (polityMembers[0] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[0]))
                    {
                        Debug.LogError("ID " + 0);
                        rootPolityMemberSerializedObj = new SerializedObject(polityMembers[0]);
                        parentsSerializedProperty = rootPolityMemberSerializedObj.FindProperty("parents");
                    }
                    else
                    {

                    }
                }
                if (rootPolityMemberSerializedObj != null)
                {
                    rootPolityMemberSerializedObj.Update(); // Make sure to update the serialized object
                    EditorGUILayout.PropertyField(parentsSerializedProperty, new GUIContent("Parents"), true);
                    rootPolityMemberSerializedObj.ApplyModifiedProperties(); // Apply properties after drawing
                }

                if (GUILayout.Button("Parents"))
                {
                    lastMemberRelation = MemberRelationType.Parent;
                    Debug.LogError("lastMember " + lastMemberRelation);
                }
                if (GUILayout.Button("Partners"))
                {
                    lastMemberRelation = MemberRelationType.Partner;
                    Debug.LogError("lastMember " + lastMemberRelation);
                }
                if (GUILayout.Button("Children"))
                {
                    lastMemberRelation = MemberRelationType.Children;
                    Debug.LogError("lastMember " + lastMemberRelation);
                }
            }
            else
            {
                if (EditorGUI.EndChangeCheck())
                {
                    if (polityMembers[id] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[id]))
                    {
                        Debug.LogError("ID nonrade" + id);
                        GetRelationToRootNode(id);

                    }
                    else
                    {

                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Attach", GUILayout.ExpandWidth(true)))
                {
                    AttachCurveToRootNode(id);
                }
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    DeleteCurveToRootNode(id);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.DragWindow();
        }

        MemberRelationType lastMemberRelation;
        Dictionary<Node, Node> linkedNodes = new();
        Dictionary<int, MemberRelationType> linkedRelationType = new();

        void AttachCurveToRootNode(int id)
        {
            if (linkedRelationType.ContainsKey(id))
            {
                return;  // If already linked, exit the method to prevent adding it again
            }
            Node root, target;
            switch (lastMemberRelation)
            {
                case MemberRelationType.Parent:
                default:
                    root = new Node(0, NodePoint.Top);
                    target = new Node(id, NodePoint.Bottom);
                    break;
                case MemberRelationType.Partner:
                    root = new Node(0, NodePoint.Right);
                    target = new Node(id, NodePoint.Left);
                    break;
                case MemberRelationType.Children:
                    root = new Node(0, NodePoint.Bottom);
                    target = new Node(id, NodePoint.Top);
                    break;
            }
            linkedNodes.Add(target, root);
            linkedRelationType.Add(id, lastMemberRelation);
            lastMemberRelation = MemberRelationType.None;
        }

        void DeleteCurveToRootNode(int id)
        {
            List<Node> keysToRemove = new();
            foreach (var pair in linkedNodes)
                if (pair.Key.NodeId == id || pair.Value.NodeId == id)
                    keysToRemove.Add(pair.Key);

            foreach (var key in keysToRemove)
            {
                linkedNodes.Remove(key);
                linkedRelationType.Remove(id);
            }
            if (keysToRemove.Count > 0)
            {
                Debug.Log("Removed " + keysToRemove.Count + " connections involving node ID " + id);
            }
        }

        void GetRelationToRootNode(int id)
        {
            if (linkedRelationType.TryGetValue(id, out MemberRelationType relation))
            {
                Debug.Log("Relation to root node is: " + relation);
                switch (relation)
                {
                    case MemberRelationType.Parent:
                        Debug.Log("This node is a parent.");
                        break;
                    case MemberRelationType.Partner:
                        Debug.Log("This node is a partner.");
                        break;
                    case MemberRelationType.Children:
                        Debug.Log("This node is a child.");
                        break;
                    default:
                        Debug.Log("Unknown relationship.");
                        break;
                }
            }
            else Debug.LogError("No relation found for ID: " + id);
        }

        void DrawNodeCurve(Rect start, Rect end, NodePoint startConnection, NodePoint endConnection)
        {
            Vector2 startPercentage = GetPercentageFromConnectionPoint(startConnection);
            Vector2 endPercentage = GetPercentageFromConnectionPoint(endConnection);
            Color lineColor = GetStartConnectionLineColor(startConnection);
            DrawNodeCurve(start, end, startPercentage, endPercentage, lineColor);
        }

        Vector2 GetPercentageFromConnectionPoint(NodePoint point)
        {
            return point switch
            {
                NodePoint.Top => new Vector2(0.5f, 0f),
                NodePoint.Right => new Vector2(1.0f, 0.5f),
                NodePoint.Left => new Vector2(0.0f, 0.5f),
                NodePoint.Bottom => new Vector2(0.5f, 1f),
                _ => new Vector2(0.5f, 0.5f),// Default to center if unknown for some reason
            };
        }

        Color GetStartConnectionLineColor(NodePoint startConnection)
        {
            return startConnection switch
            {
                NodePoint.Top => Color.blue,
                NodePoint.Right => Color.green,
                NodePoint.Left => Color.green,
                NodePoint.Bottom => Color.red,
                _ => Color.black,// Default to center if unknown for some reason
            };
        }

        void DrawNodeCurve(Rect start, Rect end, Vector2 vStartPercentage, Vector2 vEndPercentage, Color lineColor)
        {
            Vector3 startPos = new(start.x + start.width * vStartPercentage.x, start.y + start.height * vStartPercentage.y, 0);
            Vector3 endPos = new(end.x + end.width * vEndPercentage.x, end.y + end.height * vEndPercentage.y, 0);
            Vector3 startTan = startPos + Vector3.right * (-50 + 100 * vStartPercentage.x) + Vector3.up * (-50 + 100 * vStartPercentage.y);
            Vector3 endTan = endPos + Vector3.right * (-50 + 100 * vEndPercentage.x) + Vector3.up * (-50 + 100 * vEndPercentage.y);
            Color shadowCol = new(200, 200, 200, 0.4f);
            for (int i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, lineColor, null, 2);
        }

    }
}