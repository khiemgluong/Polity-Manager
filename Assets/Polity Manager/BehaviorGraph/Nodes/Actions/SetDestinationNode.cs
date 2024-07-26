using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using XNode;

namespace KhiemLuong.Actions
{
    using static BehaviorGraph;
    public class SetDestinationNode : ActionNode
    {
        [SerializeField] DestinationType destinationType;

        public Vector3 targetPosition;

        enum DestinationType
        {
            Position,
            Transform,
            PolityMember
        }

        // Node execution logic
        public TaskState SetDestination(ref BehaviorStruct _struct)
        {
            // Get the NavMeshAgent from the input
            BehaviorGraph actionGraph = graph as BehaviorGraph;
            if (actionGraph == null)
            {
                Debug.LogError("Graph is not an ActionGraph or is null");
                return TaskState.COMPLETED;
            }
            if (_struct.agent == null)
            {
                Debug.LogError("NavMeshAgent is not provided or is null");
                return TaskState.COMPLETED;
            }
            switch (destinationType)
            {
                case DestinationType.Position:
                    if (targetPosition != Vector3.zero)
                    {
                        _struct.agent.SetDestination(targetPosition);
                        Debug.LogError("set position destination to " + targetPosition);
                    }
                    break;
                case DestinationType.Transform:
                    if (_struct.targetObj != null)
                        _struct.agent.SetDestination(_struct.targetObj.position);
                    break;
                case DestinationType.PolityMember:
                    if (_struct.closestMember != null)
                    {
                        _struct.agent.SetDestination(_struct.closestMember.transform.position);
                    }
                    break;
            }
            return TaskState.SUCCEEDED;
        }
    }
}