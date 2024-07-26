using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using XNode;

namespace KhiemLuong
{
    public abstract class BehaviorNode : Node
    {
        [MinValue(0), HideInInspector]
        public int order;

        public List<NodePort> SortNodePortByOrder(NodePort outPort) =>
             outPort.GetConnections().OrderBy(p => ((BehaviorNode)p.node).order).ToList();


        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);
            BehaviorNode node = from.node as BehaviorNode;
            if (node == null)
                node = to.node as BehaviorNode;

            if (node != null) OnNodeConnected(node);
        }

        void OnNodeConnected(BehaviorNode node)
        {
            NodePort outPort = node.GetOutputPort("output");
            if (outPort != null)
            {
                List<BehaviorNode> connectedNodes = new List<BehaviorNode>();
                foreach (NodePort connectedPort in outPort.GetConnections())
                {
                    BehaviorNode outputConnectedNode = connectedPort.node as BehaviorNode;
                    if (outputConnectedNode != null)
                        connectedNodes.Add(outputConnectedNode);
                }

                connectedNodes.Sort((a, b) => a.position.y.CompareTo(b.position.y));
                for (int i = 0; i < connectedNodes.Count; i++)
                    connectedNodes[i].order = i;
            }
        }
    }
}