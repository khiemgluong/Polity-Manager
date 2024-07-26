using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace KhiemLuong
{
    using static BehaviorGraph;
    using Compositors;
    public abstract class CompositorNode : BehaviorNode
    {
        [Input] public Node input;
        [Output] public Node output;
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "output")
                return output;
            return null;
        }
        public TaskState MoveToNextNode(ref BehaviorStruct _struct)
        {
            NodePort outPort = GetOutputPort("output");
            if (outPort == null)
            {
                Debug.LogError("Output port not found in node: " + name);
                return TaskState.FAILED;
            }
            List<NodePort> connectedPorts = SortNodePortByOrder(outPort);
            foreach (NodePort connectedPort in connectedPorts)
            {
                var connectedNode = connectedPort.node;
                switch (connectedNode)
                {
                    case CompositorNode compositorNode:
                        return CallCompositorNode(ref _struct, outPort);
                    case ConditionalNode conditionalNode:
                        return conditionalNode.CallConditionalNode(ref _struct, outPort);
                    case ActionNode actionNode:
                        return actionNode.EvaluateActionNodes(ref _struct);
                }
            }
            return TaskState.COMPLETED;
        }

        /* -------------------------- CALL COMPOSITE NODES -------------------------- */
        public TaskState CallCompositorNode(ref BehaviorStruct _struct, NodePort outPort)
        {
            foreach (NodePort connectedPort in outPort.GetConnections())
            {
                var compositorNode = connectedPort.node as CompositorNode;
                if (compositorNode != null)
                {
                    switch (compositorNode)
                    {
                        case RepeaterNode repeater:
                            return repeater.RepeatNodes();
                        case SelectorNode selector:
                            return selector.SelectNodes(ref _struct);
                        case SequencerNode sequencer:
                            return sequencer.SequenceNodes(ref _struct);
                    }
                    Debug.Log($"Processing compositor node: {compositorNode.name}");
                }
            }
            return TaskState.COMPLETED;
        }
    }
}