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
                return !(ropeDot > 0.01f && pivotDot > 0.01f);
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

        [Header("GrapplingHook")]
        [SerializeField] private Transform bulletSpawnForward, bulletSpawnBackward;
        private Transform bulletSpawn => aimingBehind ? bulletSpawnBackward : bulletSpawnForward;

        // MODABLE
        [HideInInspector] public float reelRate = 2.5f;
        [HideInInspector] public List<string> hitTags;
        [HideInInspector] public BulletStats bulletStats;

        // CONSTS
        private const float minGap = 0.2f;
        private const float bodyRotationRate = 10.0f;

        // MEMBERS
        private bool attached = false;
        private float ropeLength = 0.0f;
        private float pivotLength = 0.0f;
        private List<AttachPoint> attachPoints = new List<AttachPoint>();
        private LineRenderer lineRenderer;
        private LayerMask mask;
        private bool aimingBehind;
        private Bullet projectile;

        protected override void OnEquip()
        {
            hitTags = new List<string>() { "CanGrapple", "Untagged" };
            bulletStats = BulletStats.Create(100, 10, 0);
            lineRenderer = GetComponent<LineRenderer>();
            mask = Unit.CollisionMask | (1 << (owner is Player ? 10 : 9));
        }

        protected override void OnPrimaryEnabled()
        {
            projectile = BulletPool.Fire(bulletSpawn.position, owner.AimOffset, owner.data.rb.velocity, bulletStats, true);
            //Debug.DrawRay(bulletSpawn.position, owner.AimOffset.normalized * bulletStats.range, Color.red, 1.0f);
            projectile.onHit += OnProjectileHit;
            projectile.onLost += OnProjectileLost;
        }

        protected override void OnPrimaryDisabled()
        {
            if (projectile != null) { OnProjectileLost(); }
            if (!attached) return;
            attached = false;
            owner.stateMachine.SetState(UnitState.Fall);
            attachPoints.Clear();
            lineRenderer.positionCount = 0;
            rotateFrontArm = true;
        }

        protected override void OnSecondaryEnabled()
        {

        }

        protected override void OnSecondaryDisabled()
        {

        }
        
        private void OnProjectileHit(RaycastHit2D hit)
        {
            if (projectile != null) { OnProjectileLost(); }
            if (hitTags.Contains(hit.collider.tag))
            {
                float dist = Vector2.Distance(owner.transform.position, hit.point);
                attachPoints.Add(new AttachPoint(hit.point, Vector2.zero, hit.rigidbody));
                ropeLength = dist;
                pivotLength = dist;
                owner.stateMachine.SetState(new GrappleHookState(owner.data));
                attached = true;
                rotateFrontArm = false;
                owner.data.groundSpringActive = false;

                Vector2 awayFromPivot = (hit.point - (Vector2)owner.transform.position).normalized;
                Vector2 velocityAwayFromPivot = awayFromPivot * Vector2.Dot(owner.data.rb.velocity, awayFromPivot);
                owner.data.rb.velocity -= velocityAwayFromPivot;
            }
        }

        private void OnProjectileLost()
        {
            projectile.onHit -= OnProjectileHit;
            projectile.onLost -= OnProjectileLost;
            projectile = null;
            lineRenderer.positionCount = 0;
        }

        private void FixedUpdate() {
            if (projectile != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, bulletSpawn.position);
                lineRenderer.SetPosition(1, projectile.transform.position);
            }
            
            if (!attached) return;
            if (attachPoints.Count == 0)
            {
                OnPrimaryDisabled();
                return;
            }
            OnAimPositionUpdated();

            Dictionary<int, List<AttachPoint>> toAdd = new Dictionary<int, List<AttachPoint>>();
            List<AttachPoint> toRemove = new List<AttachPoint>();
            float cummulativeLength = 0.0f;

            // Update points
            for (int i = attachPoints.Count - 1; i >= 0; --i)
            {
                // Move point with attached RB
                if (attachPoints[i].attachedRB) attachPoints[i].point += attachPoints[i].attachedRB.velocity * Time.fixedDeltaTime;
                
                Vector2 nextPoint = i == 0 ? owner.transform.position : attachPoints[i - 1].point;
                // Remove point if unwrapped
                if (i != attachPoints.Count - 1)
                {
                    Vector2 prevPoint = attachPoints[i + 1].point;
                    if (!attachPoints[i].IsWrapped(prevPoint, nextPoint))
                    {
                        toRemove.Add(attachPoints[i]);
                    }
                }
                
                float dist = Vector2.Distance(attachPoints[i].point, nextPoint);
                attachPoints[i].dist = dist;
                cummulativeLength += dist;
                //Vector2 debugPos = UnityEngine.Camera.main.WorldToScreenPoint(attachPoints[i].point);
                //Log.Text("AP" + i, i + ": " + dist, debugPos, Color.green, Time.fixedDeltaTime);

                // Get all hits between this attatch point and the next point
                List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.LinecastAll(nextPoint, attachPoints[i].point, mask));
                hits.Reverse();
                //Debug.DrawLine(attachPoints[i].point, nextPoint, i % 2 == 0 ? Color.blue : Color.red);
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
            if (secondaryActive && ropeLength > 1.0f) ropeLength -= Time.deltaTime * reelRate;
            float deltaLength = cummulativeLength - ropeLength;
            ropeLength = Mathf.Min(cummulativeLength, ropeLength);
            ropeLength = Mathf.Max(1.0f, ropeLength);
            if (deltaLength > 0.0f)
            {
                attachPoints[0].dist -= deltaLength;
                if (attachPoints[0].dist <= 0.5f)
                {
                    OnPrimaryDisabled();
                    return;
                }
            }

            // Constrain movement to pivot
            Vector2 pivot = attachPoints[0].point;
            Vector2 pivotDirection = pivot - (Vector2)owner.transform.position;
            pivotLength = attachPoints[0].dist;
            Vector2 nextPosition = (Vector2)owner.transform.position + (owner.data.rb.velocity * Time.fixedDeltaTime);
            float pivotDist = Vector2.Distance(nextPosition, pivot);
            if (pivotDist > pivotLength)
            {
                nextPosition = pivot + ((nextPosition - pivot).normalized * pivotLength);
                owner.data.rb.velocity = ((nextPosition - (Vector2)owner.transform.position) / Time.fixedDeltaTime);
                Vector2 centrifugal = -pivotDirection.normalized * 0.5f;
                //Debug.DrawRay(owner.transform.position, centrifugal, Color.magenta);
                owner.data.rb.AddForce(centrifugal, ForceMode2D.Impulse);
            }
            
            // Add new intersections
            foreach (KeyValuePair<int, List<AttachPoint>> pair in toAdd)
            {
                foreach (AttachPoint ap in pair.Value)
                {
                    attachPoints.Insert(pair.Key, ap);
                }
            }

            // Remove unwrapped points
            foreach (AttachPoint ap in toRemove)
            {
                attachPoints.Remove(ap);
                if (attachPoints.Count == 0) return;
                attachPoints[0].dist += ap.dist;
            }

            //Vector2 pos = UnityEngine.Camera.main.WorldToScreenPoint(owner.transform.position);
            //Log.Text("LEN", deltaLength.ToString(), pos, deltaLength > 0.0f ? Color.green : Color.red, Time.fixedDeltaTime);
            
            // Update Visuals
            UpdateLineRenderer();
            visualsRoot.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, owner.data.isFacingRight ? pivotDirection : -pivotDirection));
            owner.data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, visualsRoot.rotation);
            // Rotate Body
            if (owner.data.rb.velocity.magnitude > 0.5f)
            {
                Vector2 swingCross = Vector2.Perpendicular(owner.data.rb.velocity.x >= 0 ? owner.data.rb.velocity : -owner.data.rb.velocity).normalized;
                owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, Quaternion.LookRotation(Vector3.forward, pivotDirection), bodyRotationRate * Time.fixedDeltaTime);
            }
        }
        
        private void UpdateLineRenderer()
        {
            lineRenderer.positionCount = attachPoints.Count + 1;
            int j = attachPoints.Count;
            for (int i = 0; i < attachPoints.Count; ++i)
            {
                lineRenderer.SetPosition(--j, attachPoints[i].point);
            }
            lineRenderer.SetPosition(attachPoints.Count, bulletSpawn.position);
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
            return UnitState.Null;
        }
        
        public override UnitState Execute()
        {
            Vector2 velocity = data.rb.velocity;
            velocity.x += data.input.movement * data.stats.runSpeed * data.stats.airAcceleration;

            string state = string.Empty;
            const float groundCheckDist = 0.1f;
            RaycastHit2D groundHit = Physics2D.Raycast(data.rb.position, -data.rb.transform.up, data.stats.standingHalfHeight + groundCheckDist, Unit.CollisionMask);
            //Debug.DrawRay(data.rb.position, -data.rb.transform.up * (data.stats.standingHalfHeight + groundCheckDist), Color.red);
            if (groundHit.collider && Vector2.Dot(groundHit.normal, Vector2.up) >= 0.5f)
            {
                //Debug.DrawRay(data.rb.position, -data.rb.transform.up * groundHit.distance, Color.green);
                float groundInset = Mathf.Max(0.0f, -(groundHit.distance - data.stats.standingHalfHeight));
                if (groundInset > 0.0f)
                {
                    data.rb.position += (Vector2)data.rb.transform.up * groundInset;
                }
                state = velocity.magnitude > data.stats.walkSpeed * 0.5f ? "Run" : "Idle";
                data.rb.transform.rotation = Quaternion.LookRotation(Vector3.forward, groundHit.normal);
            }
            else
            {
                if (data.input.movement == 0) state = "SwingIdle";
                if (data.input.movement == 1) state = data.isFacingRight ? "SwingForward" : "SwingBackward";
                if (data.input.movement == -1) state = data.isFacingRight ? "SwingBackward" : "SwingForward";
            }
            data.animator.Play(state);
            data.rb.velocity = velocity;
            StateManager.UpdateFacing(data);
            return UnitState.Null;
        }

    }
}