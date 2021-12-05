using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class IsHungry : ActionNode
    {
        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            return context.unit.aiStats.hunger <= 0.0f ? State.Success : State.Failure;
        }
    }
}