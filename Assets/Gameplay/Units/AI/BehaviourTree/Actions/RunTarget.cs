using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class RunTarget : ActionNode
    {
        private const float successDistance = 0.5f;

        protected override void OnStart() {
            blackboard.lastPosition = context.transform.position;
        }

        protected override void OnStop() {
            context.unit.data.input.movement = 0;
        }

        protected override State OnUpdate() {
            Debug.DrawLine(context.transform.position, blackboard.target, Color.red);
            Vector2 direction = blackboard.target - (Vector2)context.transform.position;
            if (direction.x > 0.0f)
            {
                context.unit.data.input.movement = 1;
            }
            else if (direction.x < 0.0f)
            {
                context.unit.data.input.movement = -1;
            }
            
            float currentDist = Vector2.Distance(context.transform.position, blackboard.target);
            if (currentDist <= successDistance) { return State.Success; }
            
            float lastDist = Vector2.Distance(blackboard.lastPosition, blackboard.target);
            blackboard.lastPosition = context.transform.position;
            return currentDist <= lastDist ? State.Running : State.Failure;
        }
    }
}