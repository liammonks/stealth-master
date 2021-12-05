using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class Interact : ActionNode
    {
        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            return context.unit.Interact() ? State.Success : State.Failure;
        }
    }
}