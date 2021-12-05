using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class CanSeePlayer : ActionNode
    {
        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            Vector2 eyePosition = context.transform.position + (context.transform.up * (context.unit.data.isStanding ? context.unit.data.stats.standingHalfHeight : context.unit.data.stats.crawlingHalfHeight));
            Vector2 playerDirection = (Vector2)UnitHelper.Player.transform.position - eyePosition;
            LayerMask mask = Unit.CollisionMask | (1 << 9); // Add player to environment mask
            RaycastHit2D hit = Physics2D.Raycast(eyePosition, playerDirection, playerDirection.magnitude, mask);
            if(hit.collider && hit.collider.attachedRigidbody)
            {
                if (hit.collider.attachedRigidbody.GetComponent<Player>())
                {
                    return State.Success;
                }
            }
            return State.Failure;
        }
    }
}