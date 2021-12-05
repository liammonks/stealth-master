using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace BTAction
{
    public class TargetThirst : ActionNode
    {
        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            float nearestDistance = Mathf.Infinity;
            Interactable nearestInteractable = null;
            
            foreach(Interactable interactable in UnitHelper.Interactables)
            {
                if(interactable is DrinkVendor)
                {
                    float dist = Vector2.Distance(context.transform.position, interactable.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestInteractable = interactable;
                        nearestDistance = dist;
                    }
                }
            }
            
            if(nearestInteractable != null)
            {
                blackboard.target = nearestInteractable.transform.position;
                return State.Success;
            }
            
            return State.Failure;
        }
    }
}