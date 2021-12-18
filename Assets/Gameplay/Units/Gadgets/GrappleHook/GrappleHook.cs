using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class GrappleHook : BaseGadget
    {
        private const float range = 8.0f;

        private bool attatched = false;
        private Vector2 attatchPoint;
        private SpringJoint2D joint;

        protected override void OnPrimaryDisabled()
        {
            Destroy(joint);
            attatched = false;
        }

        protected override void OnPrimaryEnabled()
        {
            Vector2 direction = UnityEngine.Camera.main.ScreenToWorldPoint(Player.MousePosition) - transform.position;
            LayerMask mask = Unit.CollisionMask | (1 << (owner is Player ? 10 : 9));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, mask);
            
            if(hit.collider)
            {
                attatchPoint = hit.point;
                float jointDistance = Vector2.Distance(transform.position, attatchPoint);
                joint = owner.gameObject.AddComponent<SpringJoint2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.autoConfigureDistance = false;
                joint.connectedAnchor = attatchPoint;
                joint.enableCollision = true;
                joint.distance = jointDistance;
                joint.dampingRatio = 0.1f;
                joint.frequency = 1.0f;

                attatched = true;
            }
        }

        protected override void OnSecondaryDisabled()
        {

        }

        protected override void OnSecondaryEnabled()
        {

        }
        
        private void FixedUpdate() {
            if(attatched)
            {
                float jointDistance = Vector2.Distance(transform.position, attatchPoint);
                if(jointDistance < joint.distance - 0.1f)
                {
                    joint.distance = jointDistance + 0.05f;
                }
                Debug.DrawLine(transform.position, attatchPoint, Color.black);
            }
        }
    }
}