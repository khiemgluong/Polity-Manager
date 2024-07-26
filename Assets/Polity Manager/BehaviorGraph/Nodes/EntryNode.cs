using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace KhiemLuong
{
    using static BehaviorGraph;
    public class EntryNode : Node
    {
        [Output] public Node output;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "output")
                return output;
            return null;
        }
        public Node Execute(ref BehaviorStruct _struct)
        {
            NodePort outPort = GetOutputPort("output");
            if (outPort == null || outPort.ConnectionCount == 0)
            {
                Debug.LogError("No node connected to EntryNode's output");
                return null;
            }

            CompositorNode connectedNode = outPort.GetConnection(0).node as CompositorNode;
            connectedNode.CallCompositorNode(ref _struct, outPort);

            return connectedNode;
        }
    }
}