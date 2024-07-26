using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KhiemLuong.Conditionals
{
    using static BehaviorGraph;
    public class DistanceFromTarget : ConditionalNode
    {
        [Tooltip("If the agent is over this distance threshold, it will return a false Conditional")]
        public float threshold;
        public TaskState GetDistanceFromTarget(ref BehaviorStruct _struct)
        {
            if (_struct.closestMember != null)
            {
                if (Vector3.Distance(_struct.agent.transform.position, _struct.closestMember.transform.position) < threshold)
                {
                    Debug.LogError("Is under threshold");
                    return EvaluateConditional(ref _struct, true);
                }
                else
                {
                    Debug.LogError("is over threshold");
                    return EvaluateConditional(ref _struct, false);
                }
            }
            else
            {
                Debug.LogError("No closest member found");
                return EvaluateConditional(ref _struct, true);
            }
        }
    }
}