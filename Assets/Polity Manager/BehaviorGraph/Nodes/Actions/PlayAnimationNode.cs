using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace KhiemLuong.Actions
{
    public class PlayAnimationNode : ActionNode
    {
        public AnimationClip animation;
        public string Evaluate()
        {
            return animation.name;
        }
    }
}