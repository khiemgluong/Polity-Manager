using System.Collections;
using System.Collections.Generic;
using KhiemLuong;
using NaughtyAttributes;
using UnityEngine;
using XNode;
using static KhiemLuong.BehaviorGraph;

namespace KhiemLuong.Actions
{
    public class SeekTargetNode : ActionNode
    {
        [Layer] public int layer;
        [Tag] public string tag;
        public TaskState SeekClosestTarget(ref BehaviorStruct _struct)
        {
            Collider[] hitColliders = Physics.OverlapSphere(_struct.agent.transform.position, 10);
            GameObject _closestTarget = null;
            float minDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == layer)
                {
                    float distance = Vector3.Distance(_struct.agent.transform.position, hitCollider.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _closestTarget = hitCollider.gameObject;
                    }
                }
            }

            if (_closestTarget != null)
            {
                NodePort outPort = GetOutputPort("output");
                if (outPort == null)
                {
                    Debug.LogError("Output port not found in seek target node");
                    return TaskState.SUSPENDED;
                }

                return TaskState.SUCCEEDED;
            }
            else
            {
                Debug.LogError("Did not find any targets");
                return TaskState.FAILED;
            }
        }

   
    }
}