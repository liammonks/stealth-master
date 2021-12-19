using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class GrappleHook : BaseGadget
    {
        
        private class AttatchPoint
        {
            public Vector2 point;
            public Vector2 wrapDirection;
            
            public AttatchPoint(Vector2 a_point, Vector2 a_wrapDirection)
            {
                point = a_point;
                wrapDirection = a_wrapDirection;
            }
        }

        private const float range = 8.0f;
        private const float reelRate = 2.0f;

        private bool attatched = false;
        private List<AttatchPoint> attatchPoints = new List<AttatchPoint>();
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
            attatchPoints.Clear();
            attatched = false;
            lineRenderer.enabled = false;
        }

        protected override void OnPrimaryEnabled()
        {
            Vector2 direction = UnityEngine.Camera.main.ScreenToWorldPoint(Player.MousePosition) - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, mask);
            
            if(hit.collider)
            {
                attatchPoints.Add(new AttatchPoint(hit.point, Vector2.zero));
                float jointDistance = Vector2.Distance(transform.position, hit.point);
                joint = owner.gameObject.AddComponent<SpringJoint2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.autoConfigureDistance = false;
                joint.connectedAnchor = hit.point;
                joint.enableCollision = true;
                joint.distance = jointDistance;
                joint.dampingRatio = 0.1f;
                joint.frequency = 1.0f;

                attatched = true;
                lineRenderer.enabled = true;
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
                // Cast towards the last attatch point
                RaycastHit2D hit = Physics2D.Raycast(transform.position, attatchPoints[attatchPoints.Count - 1].point - (Vector2)transform.position, Vector2.Distance(transform.position, attatchPoints[attatchPoints.Count - 1].point) - 0.1f, mask);
                if(hit.collider)
                {
                    joint.connectedAnchor = hit.point;
                    joint.distance = Vector2.Distance(transform.position, hit.point);
                    
                    Vector3 dir = (hit.point - attatchPoints[attatchPoints.Count - 1].point).normalized;
                    Vector3 cross = Vector3.Cross(Vector3.forward, dir).normalized;
                    Vector2 deltaPos = (Vector2)transform.position - lastPos;
                    attatchPoints.Add(new AttatchPoint(hit.point, Vector2.Dot(deltaPos, cross) > 0 ? cross : -cross));
                }

                if(attatchPoints.Count > 1)
                {
                    //Debug.DrawRay(attatchPoints[attatchPoints.Count - 1].point, attatchPoints[attatchPoints.Count - 1].point - attatchPoints[attatchPoints.Count - 2].point, Color.magenta);
                    //Debug.DrawRay(attatchPoints[attatchPoints.Count - 1].point, attatchPoints[attatchPoints.Count - 1].wrapDirection, Color.red);

                    float dot = Vector2.Dot(((Vector2)transform.position - attatchPoints[attatchPoints.Count - 1].point).normalized, attatchPoints[attatchPoints.Count - 1].wrapDirection);
                    if(dot <= -0.01f)
                    {
                        attatchPoints.RemoveAt(attatchPoints.Count - 1);
                        joint.connectedAnchor = attatchPoints[attatchPoints.Count - 1].point;
                        joint.distance = Vector2.Distance(transform.position, attatchPoints[attatchPoints.Count - 1].point);
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
                    float jointDistance = Vector2.Distance(transform.position, attatchPoints[attatchPoints.Count - 1].point);
                    if (jointDistance < joint.distance - 0.1f)
                    {
                        joint.distance = jointDistance + 0.05f;
                    }
                }
                
                for (int i = 0; i < attatchPoints.Count - 1; ++i)
                {
                    Debug.DrawLine(attatchPoints[i].point, attatchPoints[i + 1].point, Color.black);
                }
                Debug.DrawLine(attatchPoints[attatchPoints.Count - 1].point, transform.position, Color.black);

                UpdateLineRenderer();
            }
            lastPos = transform.position;
        }
        
        private void UpdateLineRenderer()
        {
            Vector3[] points = new Vector3[attatchPoints.Count + 1];
            for (int i = 0; i < attatchPoints.Count; ++i)
            {
                points[i] = attatchPoints[i].point;
            }
            points[attatchPoints.Count] = transform.position;
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }
    }
}