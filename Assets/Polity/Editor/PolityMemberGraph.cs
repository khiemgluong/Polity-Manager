using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KhiemLuong
{
    public class PolityMemberGraph : EditorWindow
    {
        /* ---------------------------- POLITY VARIABLES ---------------------------- */
        enum NodePoint
        {
            Top,
            Right,
            Left,
            Bottom,
            Child
        }
        enum RelationType
        {
            Parents,
            Partners,
            Children,
        }
        RelationType relationType;
        List<PolityMember> polityMembers = new();

        /* ----------------------------- NODE RENDERERS ----------------------------- */
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
        SerializedObject serializedObject;
        GUIStyle parentNode, partnerNode, childNode;
        List<Rect> nodes = new();
        Vector2 nodeSize = new(150, 65);
        /// <summary>
        /// This nodeId is referenced only in a node which is a child of the root node
        /// </summary>
        int childNodeId = -1;
        bool isRootGenerated;
        Dictionary<Node, Node> linkedNodes = new();
        Dictionary<Node, Node> linkedChildNodes = new();
        Dictionary<int, RelationType> linkedRelationType = new();

        /* ------------------------------ PAN CONTROLS ------------------------------ */
        float panX = 0, panY = 0;
        bool isDragging = false;
        Vector2 initialMousePosition;
        Vector2 dragStartPosition;

        void OnEnable()
        {
            serializedObject = new SerializedObject(this);

            parentNode = new GUIStyle(GUI.skin.window);
            parentNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
            partnerNode = new GUIStyle(GUI.skin.window);
            partnerNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
            childNode = new GUIStyle(GUI.skin.window);
            childNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;

            nodes.Clear();
            polityMembers.Clear();
            linkedNodes.Clear();
            Vector2 windowCenter = new(position.width / 3, position.height / 3);
            Rect nodeRect = new(windowCenter.x + nodeSize.x, windowCenter.y + nodeSize.y, nodeSize.x, nodeSize.y);
            nodes.Add(nodeRect);
        }
        public static void ShowWindow()
        {
            var window = GetWindow<PolityMemberGraph>("Polity Manager");
            window.minSize = new Vector2(200, 100); // Define minimum size
            var screenResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            var windowSize = screenResolution * .33f; // Set window size to 1/3 of the screen size
            var windowPosition = (screenResolution - windowSize) * .33f; // Center the window
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
            GUILayout.BeginArea(new Rect(0, 0, sidebarWidth, sidebarHeight), "", GUI.skin.window);
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Add Node"))
            {
                if (polityMembers[0] != null)
                    nodes.Add(new Rect(nodes[0].x, nodes[0].y - 100, nodeSize.x, nodeSize.y));
                else Debug.LogWarning("You must assign a root PolityMember first");
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            /* ------------------------------- SIDEBAR END ------------------------------ */


            /* -------------------------------------------------------------------------- */
            /*                                 MAIN PANEL                                 */
            /* -------------------------------------------------------------------------- */
            GUILayout.BeginArea(new Rect(sidebarWidth, 0, mainPanelWidth, mainPanelHeight), "", GUI.skin.window);

            Rect groupRect = new(panX, panY, 10000, 10000);
            GUI.BeginGroup(groupRect);
            BeginWindows();

            foreach (var pair in linkedNodes)
                DrawNodeCurve(nodes[pair.Key.NodeId], nodes[pair.Value.NodeId], pair.Key.Point, pair.Value.Point);
            foreach (var pair in linkedChildNodes)
                DrawNodeCurve(nodes[pair.Value.NodeId], nodes[pair.Key.NodeId], pair.Value.Point, pair.Key.Point);
            for (int i = 0; i < nodes.Count; i++)
                if (i == 0)
                {
                    if (polityMembers.Any()) if (polityMembers[0] != null)
                        {
                            if (polityMembers[0].parents.Count < 2)
                                nodes[i] = new Rect(nodes[0].x, nodes[0].y, nodeSize.x, 105);
                            else nodes[i] = new Rect(nodes[0].x, nodes[0].y, nodeSize.x, 90);
                        }
                    nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Root " + i);
                }
                else
                {
                    if (linkedRelationType.ContainsKey(i))
                        switch (linkedRelationType[i])
                        {
                            case RelationType.Parents:
                                nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Parent " + i, parentNode); break;
                            case RelationType.Partners:
                                nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Partner " + i, partnerNode); break;
                            case RelationType.Children:
                                if (polityMembers[i] != null)
                                    if (polityMembers[i].parents.Count == 1)
                                        nodes[i] = new Rect(nodes[i].x, nodes[i].y, nodeSize.x, 90);
                                    else nodes[i] = new Rect(nodes[i].x, nodes[i].y, nodeSize.x, 65);
                                nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Child " + i, childNode); break;
                        }
                    else nodes[i] = GUI.Window(i, nodes[i], DrawNodeWindow, "Node " + i);
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
                if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 currentMousePosition = Event.current.mousePosition;
                    Vector2 delta = currentMousePosition - initialMousePosition;

                    panX = dragStartPosition.x + delta.x;
                    panY = dragStartPosition.y + delta.y;
                    Repaint();
                }
                else if (Event.current.type == EventType.MouseUp) isDragging = false;
        }

        void DrawNodeWindow(int id)
        {
            while (polityMembers.Count <= id) polityMembers.Add(null);
            EditorGUI.BeginChangeCheck();
            polityMembers[id] = EditorGUILayout.ObjectField("", polityMembers[id], typeof(PolityMember), false) as PolityMember;
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (id == 0)
            {
                if (polityMembers[0] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[0]))
                {
                    if (!CheckForDuplicateNode(id))
                    {
                        if (polityMembers[0].parents.Count < 2)
                            if (GUILayout.Button(RelationType.Parents.ToString()))
                                relationType = RelationType.Parents;
                        if (GUILayout.Button(RelationType.Partners.ToString()))
                            relationType = RelationType.Partners;
                        if (GUILayout.Button(RelationType.Children.ToString()))
                            relationType = RelationType.Children;
                        if (!isRootGenerated) GenerateRootNodeFamilyMembers();
                    }
                }
            }
            else
            {
                if (polityMembers[id] != null && PrefabUtility.IsPartOfPrefabAsset(polityMembers[id]))
                {
                    if (!CheckForDuplicateNode(id))
                    {
                        SetRootNodeRelationTypes(id);

                        EditorGUILayout.BeginHorizontal();
                        if (linkedRelationType.ContainsKey(id))
                        {
                            if (linkedRelationType[id] == RelationType.Partners)
                            {
                                if (GUILayout.Button("Attach", GUILayout.ExpandWidth(true)))
                                    EstablishNodeConnection(id);
                                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                                { ClearRootNodeRelations(id); DeleteCurveToRootNode(id); }
                            }
                            else
                            {
                                if (GUILayout.Button("Detach", GUILayout.ExpandWidth(true)))
                                {
                                    DeleteCurveToRootNode(id);
                                    if (linkedRelationType[id] == RelationType.Children)
                                    {
                                        PolityMember root = polityMembers[0];
                                        for (int i = 0; i < root.partners.Count; i++)
                                            for (int x = 0; x < root.partners[i].children.Count; x++)
                                                if (root.partners[i].children[x] == polityMembers[id])
                                                {
                                                    childNodeId = polityMembers.IndexOf(root.partners[i]);
                                                    DeleteCurveToParentNode(id);
                                                    ClearChildNodeRelation(id);
                                                    break;
                                                }
                                    }
                                    ClearRootNodeRelations(id);
                                }
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Attach", GUILayout.ExpandWidth(true)))
                                EstablishNodeConnection(id);
                        }
                        EditorGUILayout.EndHorizontal();
                        if (polityMembers[id] != null && nodes[id] != null)
                            if (polityMembers[id].parents.Contains(polityMembers[0]) && polityMembers[id].parents.Count < 2)
                                if (GUILayout.Button("Parent")) childNodeId = id;
                    }
                }
                else
                {
                    // polityMembers.RemoveAt(id);
                    // Debug.LogError("NULLED");
                }
            }
            GUI.DragWindow();
        }

        void EstablishNodeConnection(int id)
        {
            if (childNodeId == -1) { AttachCurveToRootNode(id); SetRootNodeRelationTypes(id); }
            else
            { AttachCurveToParentNode(id); SetNodeRelationTypes(childNodeId, id); childNodeId = -1; }
        }

        /* -------------------------------------------------------------------------- */
        /*                             NODE INITIALIZATION                            */
        /* -------------------------------------------------------------------------- */
        void GenerateRootNodeFamilyMembers()
        {
            if (polityMembers[0] == null) return;
            PolityMember root = polityMembers[0];
            root.parents = root.parents.Where(item => item != null).ToList();
            root.partners = root.partners.Where(item => item != null).ToList();
            root.children = root.children.Where(item => item != null).ToList();

            Rect rootNode = nodes[0];
            float currentXOffset;
            /* -------------------------- Building Parent Nodes ------------------------- */
            for (int i = 0; i < root.parents.Count; i++)
            {
                currentXOffset = -nodeSize.x;
                polityMembers.Add(root.parents[i]);
                if (i == 0)
                    nodes.Add(new Rect(rootNode.x + currentXOffset, rootNode.y - nodeSize.y * 2f, nodeSize.x, nodeSize.y));
                else
                {
                    currentXOffset += nodeSize.x * 2f;
                    nodes.Add(new Rect(rootNode.x + currentXOffset, rootNode.y - nodeSize.y * 2f, nodeSize.x, nodeSize.y));
                }
                relationType = RelationType.Parents;
                AttachCurveToRootNode(i + 1);
            }
            currentXOffset = nodeSize.x * 2f;
            /* ------------------------- Building Partner Nodes ------------------------- */
            List<int> partnersIds = new();
            for (int i = 0; i < root.partners.Count; i++)
            {
                polityMembers.Add(root.partners[i]);
                if (i == 0)
                    nodes.Add(new Rect(rootNode.x + currentXOffset, rootNode.y * 1.05f, nodeSize.x, nodeSize.y));
                else
                {
                    currentXOffset += nodeSize.x * 2f;
                    nodes.Add(new Rect(rootNode.x + currentXOffset, rootNode.y * 1.05f, nodeSize.x, nodeSize.y));
                }
                relationType = RelationType.Partners;
                AttachCurveToRootNode(polityMembers.Count - 1);
                partnersIds.Add(polityMembers.Count - 1);
            }
            currentXOffset = nodeSize.x / 2;
            /* ------------------------- Building Children Nodes ------------------------ */
            for (int i = 0; i < root.children.Count; i++)
            {
                polityMembers.Add(root.children[i]);
                if (i == 0)
                    nodes.Add(new Rect(rootNode.x + currentXOffset * 2f, rootNode.y + nodeSize.y * 2f, nodeSize.x, nodeSize.y));
                else
                {
                    currentXOffset += nodeSize.x * 2f;
                    nodes.Add(new Rect(rootNode.x + currentXOffset, rootNode.y + nodeSize.y * 2f, nodeSize.x, nodeSize.y));
                }
                relationType = RelationType.Children;

                int _i = polityMembers.IndexOf(root.children[i]);
                AttachCurveToRootNode(_i);
                for (int x = 0; x < partnersIds.Count; x++)
                    if (polityMembers[partnersIds[x]].children.Contains(polityMembers[_i]))
                    {
                        childNodeId = _i;
                        AttachCurveToParentNode(partnersIds[x]);
                    }
            }
            isRootGenerated = true;
        }

        /* -------------------------------------------------------------------------- */
        /*                              NODE CONNECTIONS                              */
        /* -------------------------------------------------------------------------- */

        /* -------------------------- Node Curve Attachment ------------------------- */
        void AttachCurveToRootNode(int id) => AttachCurveToNode(0, id);
        void AttachCurveToNode(int rootId, int id)
        {
            if (linkedRelationType.ContainsKey(id)) return;
            Node root, target;
            switch (relationType)
            {
                case RelationType.Parents:
                default:
                    root = new Node(rootId, NodePoint.Top);
                    target = new Node(id, NodePoint.Bottom);
                    break;
                case RelationType.Partners:
                    root = new Node(rootId, NodePoint.Right);
                    target = new Node(id, NodePoint.Left);
                    break;
                case RelationType.Children:
                    root = new Node(rootId, NodePoint.Bottom);
                    target = new Node(id, NodePoint.Top);
                    break;
            }
            if (linkedNodes.ContainsKey(target))
                linkedNodes[target] = root;
            else
                linkedNodes.Add(target, root);
            if (linkedRelationType.ContainsKey(id))
                linkedRelationType[id] = relationType;
            else
                linkedRelationType.Add(id, relationType);
        }
        void AttachCurveToParentNode(int id)
        {
            Node root = new(childNodeId, NodePoint.Top), target = new(id, NodePoint.Child);
            if (linkedRelationType.ContainsKey(id))
                if (linkedRelationType[id] == RelationType.Partners)
                {
                    if (linkedChildNodes.TryGetValue(target, out Node _target) && _target.Equals(target))
                    { Debug.LogWarning("PolityMember pair already exists."); return; }
                    else linkedChildNodes.Add(root, target);
                }
                else Debug.LogWarning("A child relation can only be made to a Partner.");
        }

        /* -------------------------- Node Curve Detachment ------------------------- */
        void DeleteCurveToNode(int rootId, int id)
        {
            List<Node> keysToRemove = new();
            foreach (var pair in linkedNodes)
                if (pair.Key.NodeId == id && pair.Value.NodeId == rootId)
                    keysToRemove.Add(pair.Key);

            foreach (var key in keysToRemove)
                linkedNodes.Remove(key);
            if (keysToRemove.Count > 0)
                Debug.Log("Removed " + keysToRemove.Count + " connections with ID " + id);
        }
        void DeleteCurveToRootNode(int id) => DeleteCurveToNode(0, id);
        void DeleteCurveToParentNode(int id)
        {
            List<Node> keysToRemove = new();
            foreach (var pair in linkedChildNodes)
                if (pair.Key.NodeId == id && pair.Value.NodeId == childNodeId)
                    keysToRemove.Add(pair.Key);

            foreach (var key in keysToRemove)
                linkedChildNodes.Remove(key);
            if (keysToRemove.Count > 0)
                Debug.Log("Removed " + keysToRemove.Count + " child connections with ID " + id);
        }

        /* ------------------------- Set Node Relation Type ------------------------- */
        void SetNodeRelationTypes(int rootId, int id)
        {
            if (polityMembers[rootId] == null)
            {
                EditorUtility.DisplayDialog(
               "Root Polity Member not assigned",
               $"Please assign a Polity Member at Node {rootId}.",
               "OK"
               );
                return;
            }
            if (polityMembers[id] == null) return;
            if (linkedRelationType.TryGetValue(id, out RelationType relation))
            {
                Debug.Log("Relation to node " + rootId + " is: " + relation + " " + id);
                switch (relation)
                {
                    case RelationType.Parents:
                        if (!polityMembers[rootId].parents.Contains(polityMembers[id]))
                            polityMembers[rootId].parents.Add(polityMembers[id]);
                        if (!polityMembers[id].children.Contains(polityMembers[rootId]))
                            polityMembers[id].children.Add(polityMembers[rootId]);
                        break;

                    case RelationType.Partners:
                        if (rootId != 0)//This is a child to partner, i.e child to parent 
                        {
                            if (!polityMembers[rootId].parents.Contains(polityMembers[id]))
                                polityMembers[rootId].parents.Add(polityMembers[id]);
                            if (!polityMembers[id].children.Contains(polityMembers[rootId]))
                                polityMembers[id].children.Add(polityMembers[rootId]);
                        }
                        else
                        {
                            if (!polityMembers[rootId].partners.Contains(polityMembers[id]))
                                polityMembers[rootId].partners.Add(polityMembers[id]);
                            if (!polityMembers[id].partners.Contains(polityMembers[rootId]))
                                polityMembers[id].partners.Add(polityMembers[rootId]);
                        }
                        break;
                    case RelationType.Children:
                        if (!polityMembers[rootId].children.Contains(polityMembers[id]))
                            polityMembers[rootId].children.Add(polityMembers[id]);
                        if (!polityMembers[id].parents.Contains(polityMembers[rootId]))
                            polityMembers[id].parents.Add(polityMembers[rootId]);
                        break;
                    default:
                        Debug.Log("Unknown relationship.");
                        break;
                }
            }
            // else Debug.LogWarning("No relation found for ID: " + id);
        }

        void SetRootNodeRelationTypes(int id) => SetNodeRelationTypes(0, id);
        /// <summary>
        /// Clears relations from the start polity Member to its linked counterpart
        /// </summary>
        void ClearChildNodeRelation(int id)
        {
            if (polityMembers[childNodeId].children.Contains(polityMembers[id]))
                polityMembers[childNodeId].children.Remove(polityMembers[id]);
            if (polityMembers[id].parents.Contains(polityMembers[childNodeId]))
                polityMembers[id].parents.Remove(polityMembers[childNodeId]);
        }
        void ClearLinkedNodeRelations(int rootId, int id)
        {
            if (linkedRelationType.ContainsKey(id))
            {
                switch (linkedRelationType[id])
                {
                    case RelationType.Partners:
                        if (polityMembers[rootId].partners.Contains(polityMembers[id]))
                            polityMembers[rootId].partners.Remove(polityMembers[id]);
                        if (polityMembers[id].partners.Contains(polityMembers[rootId]))
                            polityMembers[id].partners.Remove(polityMembers[rootId]);
                        break;
                    case RelationType.Parents:
                        if (polityMembers[rootId].parents.Contains(polityMembers[id]))
                            polityMembers[rootId].parents.Remove(polityMembers[id]);
                        if (polityMembers[id].children.Contains(polityMembers[rootId]))
                            polityMembers[id].children.Remove(polityMembers[rootId]);
                        break;
                    case RelationType.Children:
                        if (polityMembers[rootId].children.Contains(polityMembers[id]))
                            polityMembers[rootId].children.Remove(polityMembers[id]);
                        if (polityMembers[id].parents.Contains(polityMembers[rootId]))
                            polityMembers[id].parents.Remove(polityMembers[rootId]);
                        break;
                }
                linkedRelationType.Remove(id);
            }
        }
        void ClearRootNodeRelations(int id) => ClearLinkedNodeRelations(0, id);

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
                NodePoint.Child => new Vector2(0.5f, 1f),
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
                NodePoint.Child => Color.cyan,
                _ => Color.black,// Default to center if unknown for some reason
            };
        }
        void DrawNodeCurve(Rect start, Rect end, Vector2 vStartPercentage, Vector2 vEndPercentage, Color lineColor)
        {
            Vector3 startPos = new(start.x + start.width * vStartPercentage.x, start.y + start.height * vStartPercentage.y, 0);
            Vector3 endPos = new(end.x + end.width * vEndPercentage.x, end.y + end.height * vEndPercentage.y, 0);
            Vector3 startTan = startPos + Vector3.right * (-50 + 100 * vStartPercentage.x) + Vector3.up * (-50 + 100 * vStartPercentage.y);
            Vector3 endTan = endPos + Vector3.right * (-50 + 100 * vEndPercentage.x) + Vector3.up * (-50 + 100 * vEndPercentage.y);
            Color shadowCol = new(200, 200, 200, .25f);
            for (int i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, lineColor, null, 2);
        }
    }
}