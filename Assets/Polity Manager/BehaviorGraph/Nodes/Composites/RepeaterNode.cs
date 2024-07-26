using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace KhiemLuong.Compositors
{
    using static BehaviorGraph;

    public class RepeaterNode : CompositorNode
    {
        public int count;
        public bool repeatForever;
        public bool untilFailure;
        public TaskState RepeatNodes()
        {
            NodePort inputPort = GetInputPort("input");
            Node connectedInputNode = inputPort.Connection.node;

            // Execute input node logic if needed
            Debug.Log($"Preparing to execute input node: {connectedInputNode.name}");

            int executionCount = repeatForever ? int.MaxValue : count;
            for (int i = 0; i < executionCount; i++)
            {
                // Process each output node in declared order
                ExecuteNode("output");

                // Optionally, check for conditions to break the loop
                if (untilFailure)
                {
                    bool failed = CheckForFailure(); // Implement this method based on your criteria
                    if (failed) break;
                }
            }
            return TaskState.SUCCEEDED;
        }

        private void ExecuteNode(string portName)
        {
            NodePort port = GetOutputPort(portName);
            if (port != null && port.ConnectionCount > 0)
            {
                Node connectedNode = port.Connection.node;
                Debug.Log($"Executing connected node {connectedNode.name} on port {portName}");
                // Execute node logic here; you may need to cast node to specific types or call methods dynamically
            }
            else
            {
                Debug.LogError($"No node connected to port {portName} or port not found.");
            }
        }

        private bool CheckForFailure()
        {
            // Implement failure logic, returning true to indicate failure
            return false; // Placeholder implementation
        }

        public override object GetValue(NodePort port)
        {
            return null; // Return values if needed, based on your node setup
        }
    }
}