using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KhiemLuong.BehaviorGraph;

namespace KhiemLuong.Actions
{
    public class DebugLogNode : ActionNode
    {

        [NaughtyAttributes.ResizableTextArea]
        public string logString;
        enum LogLevel
        {
            Normal,
            Warning,
            Error,
        }

        [SerializeField] LogLevel logLevel;

        public TaskState DebugLog()
        {
            switch (logLevel)
            {
                case LogLevel.Normal:
                    Debug.Log(logString);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(logString);
                    break;
                case LogLevel.Error:
                    Debug.LogError(logString);
                    break;
            }
            return TaskState.SUCCEEDED;
        }
    }
}