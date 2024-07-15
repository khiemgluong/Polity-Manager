using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            Parents,
            Partners,
            Children,
        }
        MemberRelationType selectedMemberRelation;

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
        Vector2 nodeSize = new(180, 350);
        Dictionary<Node, Node> linkedNodes = new();
        Dictionary<int, MemberRelationType> linkedRelationType = new();

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
            float sidebarWidth = position.width * .1f;
            float mainPanelWidth = position.width;
            float sidebarHeight = position.height * .4f;
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

            foreach (var pair in linkedNodes)
                DrawNodeCurve(nodes[pair.Key.NodeId], nodes[pair.Value.NodeId], pair.Key.Point, pair.Value.Point);

            for (int i = 0; i < nodes.Count; i++)
                nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Node " + i);

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
                if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 currentMousePosition = Event.current.mousePosition;
                    Vector2 delta = currentMousePosition - initialMousePosition;

                    panX = dragStartPosition.x + delta.x;
                    panY = dragStartPosition.y + delta.y;
                    Repaint();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    isDragging = false;
                }
        }
        void DrawNodeWindow(int id)
        {
            while (polityMembers.Count <= id) polityMembers.Add(null);
            EditorGUI.BeginChangeCheck();
            polityMembers[id] = EditorGUILayout.ObjectField("", polityMembers[id], typeof(PolityMember), false) as PolityMember;
            if (id == 0)
            {
                if (EditorGUI.EndChangeCheck())
                {
                    if (polityMembers[0] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[0]))
                    {
                        if (!CheckForDuplicateNode(id))
                        {
                            // GenerateRootNodeFamilyMembers();
                            rootPolityMemberSerializedObj = new SerializedObject(polityMembers[0]);
                            parentsSerializedProperty = rootPolityMemberSerializedObj.FindProperty("parents");
                        }
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

                if (GUILayout.Button(MemberRelationType.Parents.ToString()))
                {
                    selectedMemberRelation = MemberRelationType.Parents;
                }
                if (GUILayout.Button(MemberRelationType.Partners.ToString()))
                {
                    selectedMemberRelation = MemberRelationType.Partners;
                }
                if (GUILayout.Button(MemberRelationType.Children.ToString()))
                {
                    selectedMemberRelation = MemberRelationType.Children;
                }
            }
            else
            {
                if (EditorGUI.EndChangeCheck())
                {
                    if (polityMembers[id] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[id]))
                    {
                        if (!CheckForDuplicateNode(id))
                        {
                            AddRelationToRootNode(id);
                        }
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
                if (polityMembers[id] != null)
                {
                    if (polityMembers[id].parents.Contains(polityMembers[0]))
                        if (GUILayout.Button(MemberRelationType.Parents.ToString()))
                        {

                        }
                    // if (linkedRelationType[id] != MemberRelationType.None)
                    // {
                    //     EditorGUILayout.LabelField("Hello");
                    // }
                }

            }
            GUI.DragWindow();
        }

        /* -------------------------------------------------------------------------- */
        /*                             NODE INITIALIZATION                            */
        /* -------------------------------------------------------------------------- */
        void GenerateRootNodeFamilyMembers()
        {
            if (polityMembers[0] == null) return; // Ensure the root node exists

            Rect rootNode = nodes[0]; // Assuming this is already correctly set somewhere in your code
            HashSet<PolityMember> uniqueMembers = new HashSet<PolityMember>();

            // Add all parents and children to the HashSet to ensure uniqueness
            foreach (var parent in polityMembers[0].parents)
            {
                uniqueMembers.Add(parent);
            }
            foreach (var child in polityMembers[0].children)
            {
                uniqueMembers.Add(child);
            }

            // Clear existing polityMembers but keep the root
            List<PolityMember> newPolityMembers = new List<PolityMember>() { polityMembers[0] };

            // Start adding from index 1
            foreach (var member in uniqueMembers)
            {
                newPolityMembers.Add(member);
                nodes.Add(new Rect(rootNode.x + nodeSize.x, rootNode.y, nodeSize.x, nodeSize.y));  // Adjust positioning as needed
            }

            // Replace old polityMembers with new one
            polityMembers = newPolityMembers;
        }

        void AttachCurveToRootNode(int id)
        {
            if (linkedRelationType.ContainsKey(id))
            {
                return;  // If already linked, exit the method to prevent adding it again
            }
            Node root, target;
            switch (selectedMemberRelation)
            {
                case MemberRelationType.Parents:
                default:
                    root = new Node(0, NodePoint.Top);
                    target = new Node(id, NodePoint.Bottom);
                    break;
                case MemberRelationType.Partners:
                    root = new Node(0, NodePoint.Right);
                    target = new Node(id, NodePoint.Left);
                    break;
                case MemberRelationType.Children:
                    root = new Node(0, NodePoint.Bottom);
                    target = new Node(id, NodePoint.Top);
                    break;
            }
            linkedNodes.Add(target, root);
            linkedRelationType.Add(id, selectedMemberRelation);
            AddRelationToRootNode(id);
            selectedMemberRelation = MemberRelationType.None;
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

        void AddRelationToRootNode(int id)
        {
            if (polityMembers[0] == null)
            {
                EditorUtility.DisplayDialog(
               "Root Polity Member not assigned",
               $"Please assign a Polity Member at Node 0.",
               "OK"
               );
                return;
            }
            if (polityMembers[id] == null) return;
            if (linkedRelationType.TryGetValue(id, out MemberRelationType relation))
            {
                Debug.Log("Relation to root node is: " + relation);
                switch (relation)
                {
                    case MemberRelationType.Parents:
                        Debug.Log("This node is a parent.");
                        break;
                    case MemberRelationType.Partners:
                        Debug.Log("This node is a partner.");
                        if (!polityMembers[0].partners.Contains(polityMembers[id]))
                            polityMembers[0].partners.Add(polityMembers[id]);
                        if (!polityMembers[id].partners.Contains(polityMembers[0]))
                            polityMembers[id].partners.Add(polityMembers[0]);
                        break;
                    case MemberRelationType.Children:
                        Debug.Log("This node is a child.");
                        if (!polityMembers[0].children.Contains(polityMembers[id]))
                            polityMembers[0].children.Add(polityMembers[id]);
                        if (!polityMembers[id].parents.Contains(polityMembers[0]))
                            polityMembers[id].parents.Add(polityMembers[0]);
                        break;
                    default:
                        Debug.Log("Unknown relationship.");
                        break;
                }
                SavePrefab(polityMembers[0].gameObject);
                SavePrefab(polityMembers[id].gameObject);
            }

            else Debug.LogError("No relation found for ID: " + id);
        }

        void ClearRelationToRootNode(int id)
        {
            if (polityMembers[0] == null)
            {
                EditorUtility.DisplayDialog(
               "Root Polity Member not assigned",
               $"Please assign a Polity Member at Node 0.",
               "OK"
               );
                return;
            }
            if (polityMembers[id] == null) return;
            if (linkedRelationType.TryGetValue(id, out MemberRelationType relation))
            {
                Debug.Log("Relation to root node is: " + relation);
                switch (relation)
                {
                    case MemberRelationType.Parents:
                        Debug.Log("This node is a parent.");
                        break;
                    case MemberRelationType.Partners:
                        Debug.Log("This node is a partner.");
                        if (!polityMembers[0].partners.Contains(polityMembers[id]))
                            polityMembers[0].partners.Add(polityMembers[id]);
                        if (!polityMembers[id].partners.Contains(polityMembers[0]))
                            polityMembers[id].partners.Add(polityMembers[0]);
                        break;
                    case MemberRelationType.Children:
                        Debug.Log("This node is a child.");
                        if (!polityMembers[0].children.Contains(polityMembers[id]))
                            polityMembers[0].children.Add(polityMembers[id]);
                        if (!polityMembers[id].parents.Contains(polityMembers[0]))
                            polityMembers[id].parents.Add(polityMembers[0]);
                        break;
                    default:
                        Debug.Log("Unknown relationship.");
                        break;
                }
                SavePrefab(polityMembers[0].gameObject);
                SavePrefab(polityMembers[id].gameObject);
            }

            else Debug.LogError("No relation found for ID: " + id);
        }

        void SavePrefab(GameObject prefab)
        {
            PrefabUtility.SaveAsPrefabAsset(prefab, AssetDatabase.GetAssetPath(prefab));
            Debug.Log("Changes saved to prefab! " + prefab.name);
        }

        bool CheckForDuplicateNode(int id)
        {
            bool isDuplicate = false;
            int i; for (i = 0; i < polityMembers.Count; i++)
                if (i != id && polityMembers[i] == polityMembers[id])
                { isDuplicate = true; break; }

            if (isDuplicate)
            {
                EditorUtility.DisplayDialog(
                               "Duplicate PolityMember Detected",
                               $"This PolityMember has already been assigned to node {i}.",
                               "OK"
                           );
                polityMembers[id] = null;
            }
            return isDuplicate;
        }


        /* -------------------------------------------------------------------------- */
        /*                            Bezier Curve Drawers                            */
        /* -------------------------------------------------------------------------- */
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