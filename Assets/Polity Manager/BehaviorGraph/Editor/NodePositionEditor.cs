using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace KhiemLuong
{
    [CustomNodeEditor(typeof(BehaviorNode))]
    public abstract class NodePositionEditor : NodeEditor
    {
        Vector2 lastPosition = Vector2.zero;
        public override void OnHeaderGUI()
        {
            var node = target as BehaviorNode;
            // Call base method
            base.OnHeaderGUI();
            // Check if node position has changed
            if (node != null)
            {
                Vector2 newPosition = node.position;
                if (newPosition != lastPosition)
                {
                    OnNodeMoved(node);
                    lastPosition = newPosition;
                }
            }
        }

        private void OnNodeMoved(BehaviorNode node)
        {
            NodePort inPort = node.GetInputPort("input");
            if (inPort != null && inPort.Connection != null)
            {
                Node connectedNode = inPort.Connection.node;
                NodePort outPort = inPort.Connection;

                List<BehaviorNode> connectedNodes = new List<BehaviorNode>();
                // Get all nodes connected to the output port of the connected node
                foreach (NodePort connectedPort in outPort.GetConnections())
                {
                    BehaviorNode outputConnectedNode = connectedPort.node as BehaviorNode;
                    if (outputConnectedNode != null)
                        connectedNodes.Add(outputConnectedNode);
                }

                if (connectedNodes.Count == 0)
                {
                    // If there are no connected nodes, set the moved node's order to 0
                    node.order = 0;
                }
                else
                {
                    // Sort the connected nodes by their Y position
                    connectedNodes.Sort((a, b) => a.position.y.CompareTo(b.position.y));
                    for (int i = 0; i < connectedNodes.Count; i++)
                    {
                        connectedNodes[i].order = i;
                    }
                }
            }
            else
            {
                Debug.Log("Input port is null or has no connections.");
                node.order = 0;
            }
        }
    }
}