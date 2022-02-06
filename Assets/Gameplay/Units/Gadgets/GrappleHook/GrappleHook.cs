using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using States;

namespace Gadgets
{
    public class GrappleHook : BaseGadget
    {
        
        private class AttachPoint
        {
            public Vector2 point;
            public Rigidbody2D attachedRB;
            public float dist;

            private Vector2 hitNormal;

            public AttachPoint(Vector2 a_point, Vector2 a_hitNormal, Rigidbody2D a_attachedRB)
            {
                point = a_point;
                hitNormal = a_hitNormal;
                attachedRB = a_attachedRB;
            }
            
            public bool IsWrapped(Vector2 prevPoint, Vector2 nextPoint)
            {
                Vector2 dir1 = point - prevPoint;
                Vector2 dir2 = nextPoint - point;
                float ropeDot = Vector2.Dot(dir1, dir2);
                float pivotDot = Vector2.Dot(GetNormal(dir1), dir2);
                //Debug.DrawRay(point, hitNormal * 0.5f, (ropeDot > 0 && pivotDot > 0) ? Color.green : Color.red);
                return !(ropeDot > 0 && pivotDot > 0);
            }
            
            private Vector2 GetNormal(Vector2 dir)
            {
                Vector2 norm1 = Vector3.Cross(dir, Vector3.forward).normalized;
                Vector2 norm2 = Vector3.Cross(dir, Vector3.back).normalized;
                float dot1 = Vector2.Dot(norm1, hitNormal);
                float dot2 = Vector2.Dot(norm2, hitNormal);
                Debug.DrawRay(point, dot1 > dot2 ? norm1 : norm2, Color.magenta);
                return dot1 > dot2 ? norm1 : norm2;
            }
        }

        private const float range = 18.0f;
        private const float minGap = 0.2f;
        private const float reelRate = 2.5f;

        private bool attached = false;
        private float ropeLength = 0.0f;
        private float pivotLength = 0.0f;
        private List<AttachPoint> attachPoints = new List<AttachPoint>();
        private LineRenderer lineRenderer;
        private LayerMask mask;

        protected override void OnEquip()
        {
            lineRenderer = GetComponent<LineRenderer>();
            mask = Unit.CollisionMask | (1 << (owner is Player ? 10 : 9));
        }

        protected override void OnPrimaryEnabled()
        {
            Vector2 direction = UnityEngine.Camera.main.ScreenToWorldPoint(Player.MousePosition) - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, mask);
            
            if(hit.collider)
            {
                attachPoints.Add(new AttachPoint(hit.point, Vector2.zero, hit.rigidbody));
                ropeLength = hit.distance;
                pivotLength = ropeLength;
                owner.stateMachine.SetState(new GrappleHookState(owner.data));
                attached = true;
            }
        }

        protected override void OnPrimaryDisabled()
        {
            attached = false;
            owner.stateMachine.SetState(UnitState.Fall);
            attachPoints.Clear();
        }

        protected override void OnSecondaryEnabled()
        {

        }

        protected override void OnSecondaryDisabled()
        {

        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (!attached) return;

            Dictionary<int, List<AttachPoint>> toAdd = new Dictionary<int, List<AttachPoint>>();
            List<AttachPoint> toRemove = new List<AttachPoint>();
            float cummulativeLength = 0.0f;

            // Update points
            for (int i = attachPoints.Count - 1; i >= 0; --i)
            {
                // Move point with attached RB
                if (attachPoints[i].attachedRB) attachPoints[i].point += attachPoints[i].attachedRB.velocity * Time.fixedDeltaTime;
                
                Vector2 nextPoint = i == 0 ? transform.position : attachPoints[i - 1].point;
                // Remove point if unwrapped
                if (i != attachPoints.Count - 1)
                {
                    Vector2 prevPoint = attachPoints[i + 1].point;
                    if (!attachPoints[i].IsWrapped(prevPoint, nextPoint))
                    {
                        toRemove.Add(attachPoints[i]);
                    }
                    //Vector2 debugPos = UnityEngine.Camera.main.WorldToScreenPoint(attachPoints[i].point);
                    //Log.Text("AP" + i, ropeDot + ": " + pivotDot, debugPos, Color.green, Time.fixedDeltaTime);
                }
                
                float dist = Vector2.Distance(attachPoints[i].point, nextPoint);
                attachPoints[i].dist = dist;
                cummulativeLength += dist;

                // Get all hits between this attatch point and the next point
                List<RaycastHit2D> hits = new List<RaycastHit2D>();
                hits.AddRange(Physics2D.LinecastAll(attachPoints[i].point, nextPoint, mask));
                hits.AddRange(Physics2D.LinecastAll(nextPoint, attachPoints[i].point, mask));
                Debug.DrawLine(attachPoints[i].point, nextPoint, i % 2 == 0 ? Color.blue : Color.red);
                // Add a new point if not too close to an existing point
                foreach (RaycastHit2D hit in hits)
                {
                    bool addPoint = true;
                    // Check current attachPoints
                    foreach (AttachPoint ap in attachPoints)
                    {
                        if (Vector2.Distance(hit.point, ap.point) < minGap)
                        {
                            addPoint = false;
                            break;
                        }
                    }
                    // Check newly added points
                    if (toAdd.ContainsKey(i))
                    {
                        foreach (AttachPoint ap in toAdd[i])
                        {
                            if (Vector2.Distance(hit.point, ap.point) < minGap)
                            {
                                addPoint = false;
                                break;
                            }
                        }
                    }
                    if (addPoint)
                    {
                        if (!toAdd.ContainsKey(i))
                        {
                            toAdd.Add(i, new List<AttachPoint>());
                        }
                        toAdd[i].Add(new AttachPoint(hit.point, GetHitNormal(hit.point), hit.rigidbody));
                    }
                }
            }

            // Reel in
            if (secondaryActive) ropeLength -= Time.deltaTime * reelRate;
            float deltaLength = cummulativeLength - ropeLength;
            ropeLength = Mathf.Min(cummulativeLength, ropeLength);
            if (deltaLength > 0.0f)
            {
                attachPoints[0].dist -= deltaLength;
            }
            //if (attachPoints[0].dist < 1.0f)
            //{
            //    OnPrimaryDisabled();
            //    return;
            //}

            // Constrain movement to pivot
            Vector2 pivot = attachPoints[0].point;
            pivotLength = attachPoints[0].dist;
            Vector2 nextPosition = (Vector2)transform.position + (owner.data.rb.velocity * Time.fixedDeltaTime);
            if (Vector2.Distance(nextPosition, pivot) > pivotLength)
            {
                nextPosition = pivot + ((nextPosition - pivot).normalized * pivotLength);
                owner.data.rb.velocity = (nextPosition - (Vector2)transform.position) / Time.fixedDeltaTime;
            }

            // Remove unwrapped points
            foreach (AttachPoint ap in toRemove)
            {
                attachPoints.Remove(ap);
            }

            // Add new intersections
            foreach (KeyValuePair<int, List<AttachPoint>> pair in toAdd)
            {
                foreach (AttachPoint ap in pair.Value)
                {
                    attachPoints.Insert(pair.Key, ap);
                }
            }

            //Vector2 pos = UnityEngine.Camera.main.WorldToScreenPoint(((Vector2)transform.position + pivot) / 2);
            //Log.Text("LEN", pivotLength + "/" + ropeLength, pos, Color.green, Time.fixedDeltaTime);
            UpdateLineRenderer();
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
        
        private Vector2 GetHitNormal(Vector2 point)
        {
            const float dist = 0.1f;
            Vector2 tl = new Vector2(point.x - dist, point.y + dist);
            Vector2 tr = new Vector2(point.x + dist, point.y + dist);
            Vector2 bl = new Vector2(point.x - dist, point.y - dist);
            Vector2 br = new Vector2(point.x + dist, point.y - dist);
            Vector2 avg = Vector2.zero;

            RaycastHit2D tltr = Physics2D.Linecast(tl, tr, mask);
            if (tltr.collider && tltr.point != tl) avg += Vector2.left;
            RaycastHit2D trtl = Physics2D.Linecast(tr, tl, mask);
            if (trtl.collider && trtl.point != tr) avg += Vector2.right;
            RaycastHit2D trbr = Physics2D.Linecast(tr, br, mask);
            if (trbr.collider && trbr.point != tr) avg += Vector2.up;
            RaycastHit2D brtr = Physics2D.Linecast(br, tr, mask);
            if (brtr.collider && brtr.point != br) avg += Vector2.down;
            RaycastHit2D brbl = Physics2D.Linecast(br, bl, mask);
            if (brbl.collider && brbl.point != br) avg += Vector2.right;
            RaycastHit2D blbr = Physics2D.Linecast(bl, br, mask);
            if (blbr.collider && blbr.point != bl) avg += Vector2.left;
            RaycastHit2D bltl = Physics2D.Linecast(bl, tl, mask);
            if (bltl.collider && bltl.point != bl) avg += Vector2.down;
            RaycastHit2D tlbl = Physics2D.Linecast(tl, bl, mask);
            if (tlbl.collider && tlbl.point != tl) avg += Vector2.up;

            return avg.normalized;
        }
    }

    public class GrappleHookState : BaseState
    {
        public GrappleHookState(UnitData a_data) : base(a_data) { }

        public override UnitState Initialise()
        {
            data.animator.Play("Fall");
            return UnitState.Null;
        }
        
        public override UnitState Execute()
        {
            Vector2 velocity = data.rb.velocity;
            velocity.x += data.input.movement * data.stats.walkSpeed * data.stats.airAcceleration;
            data.rb.velocity = velocity;
            return UnitState.Null;
        }

    }
}