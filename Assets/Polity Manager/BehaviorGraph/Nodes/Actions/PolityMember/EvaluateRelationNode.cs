using System.Collections;
using UnityEngine;
using XNode;

namespace KhiemLuong.Actions
{
    using static BehaviorGraph;
    using static PolityManager;
    public class EvaluateRelationNode : ActionNode
    {
        public TaskState Execute(ref BehaviorStruct _struct)
        {
            PolityMember closestMember = _struct.closestMember;
            if (closestMember != null)
            {
                PolityMember thisMember = _struct.agent.GetComponent<PolityMember>();
                PolityRelation relation = PM.GetPolityRelation(thisMember, closestMember);
                switch (relation)
                {
                    case PolityRelation.Neutral:
                        _struct.agent.speed = 1.8f;
                        Debug.LogError("Relation is neutral");
                        // _struct.targetObj = closestMember.transform;
                        break;
                    case PolityRelation.Allies:
                        break;
                    case PolityRelation.Enemies:
                        break;
                }

                return TaskState.SUCCEEDED;
            }
            else return TaskState.FAILED;
        }
    }
}