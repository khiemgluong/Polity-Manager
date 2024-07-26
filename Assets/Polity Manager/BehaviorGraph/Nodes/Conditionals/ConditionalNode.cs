using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace KhiemLuong
{
    using static BehaviorGraph;
    using KhiemLuong.Conditionals;

    public abstract class ConditionalNode : BehaviorNode
    {
        [Input] public Node input;
        [Output,] public Node output;

        /// <summary>
        /// Conditional nodes are like booleans, they only allow 2 node connections, the first is true and the second false
        /// </summary>
        protected override void Init() => base.Init();
        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);
            if (from.node is ConditionalNode && from.fieldName == "output" && from.ConnectionCount > 2)
            {
                from.Disconnect(to);
                Debug.LogWarning("Connection limit reached for output port.");
            }
        }
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "output")
                return output;
            return null;
        }

        /* --------------------------- EVALUATE CONDITIONS -------------------------- */
        public TaskState EvaluateConditional(ref BehaviorStruct _struct, bool _bool)
        {
            NodePort outPort = GetOutputPort("output");
            if (outPort == null)
            {
                Debug.LogError("Output port not found in node: " + name);
                return TaskState.FAILED;
            }
            List<NodePort> connectedPorts = SortNodePortByOrder(outPort);
            if (_bool)
            {
                if (connectedPorts[0] != null)
                {
                    var connectedNode = connectedPorts[0].node as BehaviorNode;
                    switch (connectedNode)
                    {
                        case CompositorNode compositorNode:
                            return compositorNode.CallCompositorNode(ref _struct, outPort);
                        case ConditionalNode:
                            return CallConditionalNode(ref _struct, outPort);
                        case ActionNode actionNode:
                            return actionNode.EvaluateActionNodes(ref _struct);
                    }
                    Debug.LogError("Calling true case in conditional");
                }
            }
            else
            {
                if (connectedPorts[1] != null)
                {
                    var connectedNode = connectedPorts[1].node as BehaviorNode;
                    switch (connectedNode)
                    {
                        case CompositorNode compositorNode:
                            return compositorNode.CallCompositorNode(ref _struct, outPort);
                        case ConditionalNode:
                            return CallConditionalNode(ref _struct, outPort);
                        case ActionNode actionNode:
                            return actionNode.EvaluateActionNodes(ref _struct);
                    }
                    Debug.LogError("Calling false case in conditional " + connectedNode.name);
                }
            }
            return TaskState.FAILED;
        }

        public TaskState CallConditionalNode(ref BehaviorStruct _struct, NodePort outPort)
        {
            foreach (NodePort connectedPort in outPort.GetConnections())
            {
                var compositorNode = connectedPort.node as ConditionalNode;
                if (compositorNode != null)
                {
                    switch (compositorNode)
                    {
                        case DistanceFromTarget distanceFromTarget:
                            return distanceFromTarget.GetDistanceFromTarget(ref _struct);
                        case IsFollowingTarget isFollowingTarget:
                            return isFollowingTarget.GetIsFollowingTarget(ref _struct);
                        case IsWithinViewCone isWithinViewCone:
                            return isWithinViewCone.GetIsWithinViewCone(ref _struct);
                    }
                    Debug.LogError($"Processing conditional node: {compositorNode.name}");
                }
            }
            return TaskState.FAILED;
        }
    }
}