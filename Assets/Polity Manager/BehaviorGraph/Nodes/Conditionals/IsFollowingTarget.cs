using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;


namespace KhiemLuong.Conditionals
{
    using static BehaviorGraph;
    public class IsFollowingTarget : ConditionalNode
    {
        // Start is called before the first frame update
        /// <summary>
        /// Checks if the _struct.closestMember is not null
        /// </summary>
        public TaskState GetIsFollowingTarget(ref BehaviorStruct _struct)
        {
            if (_struct.closestMember != null)
            {
                Debug.LogError("Is following target");
                return EvaluateConditional(ref _struct, true);
            }
            else
            {
                Debug.LogError("Is not following target");
                return EvaluateConditional(ref _struct, false);
            }
        }
    }
}