using System.Collections;
using System.Collections.Generic;
using KhiemLuong;
using UnityEngine;
using XNode;
namespace KhiemLuong.Actions
{
    using static BehaviorGraph;
    public class SeekPolityMemberNode : ActionNode
    {
        [SerializeField] float seekRadius = 10;
        public TaskState SeekClosestPolityMember(ref BehaviorStruct _struct)
        {
            Collider[] hitColliders = Physics.OverlapSphere(_struct.agent.transform.position, seekRadius);
            PolityMember _closestMember = null;
            float minDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
                if (hitCollider.TryGetComponent<PolityMember>(out var polityMember))
                    if (_struct.agent.TryGetComponent<PolityMember>(out var member))
                    {
                        if (polityMember != member)
                        {
                            float distance = Vector3.Distance(_struct.agent.transform.position, polityMember.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                _closestMember = polityMember;
                            }
                        }
                    }

            if (_closestMember != null)
            {
                Debug.LogError("Found closest polity member");
                _struct.closestMember = _closestMember;
                return TaskState.SUCCEEDED;
            }
            else
            {
                Debug.LogError("Did not find any targets");
                return TaskState.FAILED;
            }
        }

        // public override object GetValue(NodePort port)
        // {
        //     if (port.fieldName == "output")
        //     {
        //         return output;
        //     }
        //     return null; // Return null if port not recognized
        // }
    }
}