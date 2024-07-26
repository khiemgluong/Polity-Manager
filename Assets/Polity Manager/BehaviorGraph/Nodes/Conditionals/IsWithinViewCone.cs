using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace KhiemLuong.Conditionals
{
    using static BehaviorGraph;
    public class IsWithinViewCone : ConditionalNode
    {

        [MinValue(15), MaxValue(270)]
        [Tooltip("If the agent target is outside the cone, it will return a false Conditional")]
        public float degreeRange = 180;
        public TaskState GetIsWithinViewCone(ref BehaviorStruct _struct)
        {
            Vector3 forward = _struct.agent.transform.forward;  // Already in local space; transform if needed
            Vector3 toOther = (_struct.closestMember.transform.position - _struct.agent.transform.position).normalized;

            float dot = Vector3.Dot(forward, toOther);
            float angleInRadians = Mathf.Acos(dot); // Get the angle in radians from the cosine value
            float angleInDegrees = angleInRadians * Mathf.Rad2Deg; // Convert radians to degrees

            Vector3 cross = Vector3.Cross(forward, toOther);
            if (cross.y < 0) angleInDegrees = 360 - angleInDegrees;

            if (IsAngleWithinCone(angleInDegrees, degreeRange))
            {
                Debug.LogError("Collision on capsule is inside the cone.");
                return EvaluateConditional(ref _struct, true);
            }
            else
            {
                Debug.LogError("Collision on capsule is outside the cone.");
                return EvaluateConditional(ref _struct, false);
            }
        }

        bool IsAngleWithinCone(float angle, float degreeRange)
        {
            angle = NormalizeAngle(angle);
            float halfRange = degreeRange / 2;
            halfRange = Mathf.Abs(halfRange);
            Debug.LogError("Angle " + angle + " " + halfRange);

            return angle <= halfRange || angle >= 360 - halfRange;
        }

        float NormalizeAngle(float angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }
    }
}