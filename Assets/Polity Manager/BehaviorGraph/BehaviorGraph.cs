using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using XNode;

namespace KhiemLuong
{
    [CreateAssetMenu(fileName = "", menuName = "Node Graphs/Behavior Graph")]

    public class BehaviorGraph : NodeGraph
    {
        protected EntryNode root;

        public enum TaskState
        {
            NONE,
            SUCCEEDED,
            COMPLETED,
            FAILED,
            ABORTED,
            SUSPENDED,
        }

        public struct BehaviorStruct
        {
            public NavMeshAgent agent;
            public Animator animator;
            public Transform targetObj;
            //Non generic classes
            public PolityMember closestMember;
        }

        public void Initialize(ref BehaviorStruct _struct)
        {
            root = nodes.Find(x => x is EntryNode && x.Inputs.All(y => !y.IsConnected)) as EntryNode;
            root.Execute(ref _struct);
            Debug.Log("initialized behavior graph");
        }
        public void Restart(ref BehaviorStruct _struct)
        {
            root.Execute(ref _struct);
        }


        /* ----------------------------- ON NODE DELETED ---------------------------- */
        public override void RemoveNode(Node node)
        {
            if (node is BehaviorNode behaviorNode)
                OnNodeDeleted(behaviorNode);
            base.RemoveNode(node);
        }

        void OnNodeDeleted(BehaviorNode node)
        {
            NodePort inPort = node.GetInputPort("input");
            if (inPort != null && inPort.Connection != null)
            {
                Node connectedNode = inPort.Connection.node;
                NodePort outPort = inPort.Connection;

                List<BehaviorNode> connectedNodes = new();

                // Get all nodes connected to the output port of the connected node
                foreach (NodePort connectedPort in outPort.GetConnections())
                {
                    BehaviorNode outputConnectedNode = connectedPort.node as BehaviorNode;
                    if (outputConnectedNode != null && outputConnectedNode != node)
                    {
                        connectedNodes.Add(outputConnectedNode);
                    }
                }

                connectedNodes.Sort((a, b) => a.position.y.CompareTo(b.position.y));
                for (int i = 0; i < connectedNodes.Count; i++)
                {
                    connectedNodes[i].order = i;
                }
            }
            else
            {
                Debug.LogError("Input port is null or has no connections.");
                node.order = 0;
            }
        }
    }
}