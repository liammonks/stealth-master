using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class TargetLeft : ActionNode
    {
        protected override void OnStart() {

        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            RaycastHit2D hit = Physics2D.Raycast(
                context.transform.position + (Vector3.left * context.unit.data.stats.standingHalfHeight * 2),
                Vector2.down,
                context.unit.data.stats.standingHalfHeight + context.unit.aiStats.maxFallDistance,
                Unit.CollisionMask
            );
            if (hit)
            {
                blackboard.target = hit.point + (Vector2.up * context.unit.data.stats.standingHalfHeight);
            }
            return hit ? State.Success : State.Failure;
        }
    }
}