using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace KhiemLuong.Compositors
{
    using static BehaviorGraph;
    public class SelectorNode : CompositorNode
    {
        public TaskState SelectNodes(ref BehaviorStruct _struct)
        {
            NodePort outPort = GetOutputPort("output");
            if (outPort == null)
            {
                Debug.LogError("Output port not found in selector");
                return TaskState.COMPLETED;
            }
            List<NodePort> connectedPorts = SortNodePortByOrder(outPort);
            TaskState taskState = TaskState.NONE;
            foreach (NodePort connectedPort in connectedPorts)
            {
                var connectedNode = connectedPort.node;
                switch (connectedNode)
                {
                    case CompositorNode compositorNode:
                        taskState = CallCompositorNode(ref _struct, outPort);
                        break;
                    case ConditionalNode conditionalNode:
                        taskState = conditionalNode.CallConditionalNode(ref _struct, outPort);
                        break;
                    case ActionNode actionNode:
                        taskState = actionNode.EvaluateActionNodes(ref _struct);
                        Debug.LogError("task selector " + taskState + " " + actionNode.name);
                        break;
                }
                if (taskState == TaskState.SUCCEEDED)
                {
                    Debug.LogError("Node Succeeded " + connectedNode.name);
                    return taskState;
                }
            }
            return taskState;
        }
    }
}