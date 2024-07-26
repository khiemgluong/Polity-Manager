using System.Collections;
using System.Collections.Generic;
using KhiemLuong;
using UnityEngine;
using XNode;

namespace KhiemLuong
{
    using static BehaviorGraph;
    using Actions;
    public abstract class ActionNode : BehaviorNode
    {
        [Input] public Node input;

        /* ---------------------------- CALL ACTION NODES --------------------------- */
        public TaskState EvaluateActionNodes(ref BehaviorStruct _struct)
        {
            TaskState taskState = TaskState.NONE;
            if (this != null)
            {
                switch (this)
                {
                    case SeekPolityMemberNode seekPolityMember:
                        taskState = seekPolityMember.SeekClosestPolityMember(ref _struct);
                        break;
                    case SeekTargetNode seekTarget:
                        taskState = seekTarget.SeekClosestTarget(ref _struct);
                        break;
                    case SetDestinationNode setDestination:
                        taskState = setDestination.SetDestination(ref _struct);
                        break;
                    case EvaluateRelationNode evaluatePolityRelation:
                        taskState = evaluatePolityRelation.Execute(ref _struct);
                        break;
                    case DebugLogNode debugLog:
                        taskState = debugLog.DebugLog(); break;
                }
                Debug.LogError($"Processing action node: {name}");
            }
            else
            {
                Debug.LogError("Connected node is null or invalid.");
            }
            return taskState;
        }
    }
}