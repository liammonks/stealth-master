using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class GrappleHook : BaseGadget
    {
        
        private class AttachPoint
        {
            public Vector2 point;
            public Vector2 wrapDirection;
            
            public AttachPoint(Vector2 a_point, Vector2 a_wrapDirection)
            {
                point = a_point;
                wrapDirection = a_wrapDirection;
            }
        }

        private const float freq = 0.5f;
        private const float range = 8.0f;
        private const float reelRate = 2.5f;

        private bool attached = false;
        private List<AttachPoint> attachPoints = new List<AttachPoint>();
        private SpringJoint2D joint;
        private LineRenderer lineRenderer;
        private LayerMask mask;
        private Vector2 lastPos;

        protected override void OnEquip()
        {
            lineRenderer = GetComponent<LineRenderer>();
            mask = Unit.CollisionMask | (1 << (owner is Player ? 10 : 9));
            lastPos = transform.position;
        }

        protected override void OnPrimaryDisabled()
        {
            Destroy(joint);
            owner.SetState(UnitState.Idle);
            attachPoints.Clear();
            attached = false;
            lineRenderer.enabled = false;
            owner.data.groundSpringActive = true;
        }

        protected override void OnPrimaryEnabled()
        {
            Vector2 direction = UnityEngine.Camera.main.ScreenToWorldPoint(Player.MousePosition) - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, mask);
            
            if(hit.collider)
            {
                attachPoints.Add(new AttachPoint(hit.point, Vector2.zero));
                float jointDistance = Vector2.Distance(transform.position, hit.point);
                joint = owner.gameObject.AddComponent<SpringJoint2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.autoConfigureDistance = false;
                joint.connectedAnchor = hit.point;
                joint.enableCollision = true;
                joint.distance = jointDistance;
                joint.dampingRatio = 0.1f;
                joint.frequency = freq;

                owner.SetState(UnitState.GrappleHookSwing);
                attached = true;
                lineRenderer.positionCount = 0;
                lineRenderer.enabled = true;
                owner.data.groundSpringActive = false;
            }
        }

        protected override void OnSecondaryDisabled()
        {

        }

        protected override void OnSecondaryEnabled()
        {
        
        }
        
        protected override void FixedUpdate() {
            base.FixedUpdate();
            if(attached)
            {
                joint.frequency = Mathf.Clamp((Vector2.Dot((joint.connectedAnchor - (Vector2)transform.position).normalized, Vector2.up) + 1) * freq, 0.00001f, float.MaxValue);
                
                // Cast towards the last attatch point
                RaycastHit2D hit = Physics2D.Raycast(transform.position, attachPoints[attachPoints.Count - 1].point - (Vector2)transform.position, Vector2.Distance(transform.position, attachPoints[attachPoints.Count - 1].point) - 0.1f, mask);
                if(hit.collider)
                {
                    joint.connectedAnchor = hit.point;
                    joint.distance = Vector2.Distance(transform.position, hit.point) - 0.2f;
                    
                    Vector3 dir = (hit.point - attachPoints[attachPoints.Count - 1].point).normalized;
                    Vector3 cross = Vector3.Cross(Vector3.forward, dir).normalized;
                    Vector2 deltaPos = (Vector2)transform.position - lastPos;
                    attachPoints.Add(new AttachPoint(hit.point, Vector2.Dot(deltaPos, cross) > 0 ? cross : -cross));
                }

                if(attachPoints.Count > 1)
                {
                    //Debug.DrawRay(attatchPoints[attatchPoints.Count - 1].point, attatchPoints[attatchPoints.Count - 1].point - attatchPoints[attatchPoints.Count - 2].point, Color.magenta);
                    //Debug.DrawRay(attatchPoints[attatchPoints.Count - 1].point, attatchPoints[attatchPoints.Count - 1].wrapDirection, Color.red);

                    float dot = Vector2.Dot(((Vector2)transform.position - attachPoints[attachPoints.Count - 1].point).normalized, attachPoints[attachPoints.Count - 1].wrapDirection);
                    if(dot <= -0.01f)
                    {
                        attachPoints.RemoveAt(attachPoints.Count - 1);
                        joint.connectedAnchor = attachPoints[attachPoints.Count - 1].point;
                        joint.distance = Vector2.Distance(transform.position, attachPoints[attachPoints.Count - 1].point) - 0.2f;
                    }
                }

                if(secondaryActive)
                {
                    joint.distance -= Time.fixedDeltaTime * reelRate;
                    if(joint.distance <= 0)
                    {
                        DisablePrimary();
                    }
                }
                else
                {
                    //float jointDistance = Vector2.Distance(transform.position, attachPoints[attachPoints.Count - 1].point);
                    //if (jointDistance < joint.distance - 0.1f)
                    //{
                    //    joint.distance = jointDistance + 0.05f;
                    //}
                }
                
                for (int i = 0; i < attachPoints.Count - 1; ++i)
                {
                    Debug.DrawLine(attachPoints[i].point, attachPoints[i + 1].point, Color.black);
                }
                Debug.DrawLine(attachPoints[attachPoints.Count - 1].point, transform.position, Color.black);

                UpdateLineRenderer();
            }
            lastPos = transform.position;
        }
        
        private void UpdateLineRenderer()
        {
            Vector3[] points = new Vector3[attachPoints.Count + 1];
            for (int i = 0; i < attachPoints.Count; ++i)
            {
                points[i] = attachPoints[i].point;
            }
            points[attachPoints.Count] = transform.position;
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }
    }
}