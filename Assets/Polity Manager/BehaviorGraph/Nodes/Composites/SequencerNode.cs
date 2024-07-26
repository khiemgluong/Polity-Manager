using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace KhiemLuong.Compositors
{
    using static BehaviorGraph;
    public class SequencerNode : CompositorNode
    {
        public TaskState SequenceNodes(ref BehaviorStruct _struct)
        {
            NodePort outPort = GetOutputPort("output");
            if (outPort == null)
            {
                Debug.LogError("Output port not found in sequencer.");
                return TaskState.COMPLETED;
            }

            List<NodePort> connectedPorts = SortNodePortByOrder(outPort);
            TaskState taskState = TaskState.NONE;
            foreach (NodePort connectedPort in connectedPorts)
            {
                var connectedNode = connectedPort.node as BehaviorNode;
                switch (connectedNode)
                {
                    case ActionNode actionNode:
                        taskState = actionNode.EvaluateActionNodes(ref _struct);
                        break;
                    case ConditionalNode conditionalNode:
                        taskState = conditionalNode.CallConditionalNode(ref _struct, outPort);
                        break;
                    case CompositorNode compositorNode:
                        taskState = CallCompositorNode(ref _struct, outPort);
                        break;
                }
                if (taskState == TaskState.FAILED)
                {
                    Debug.LogError("Sequence Compositor Failed at " + connectedNode.name);
                    return TaskState.FAILED;
                }

            }
            return taskState;

        }
    }
}