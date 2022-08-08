using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    private class Intersection
    {

    }

    #region Inspector
    [SerializeField]
    private float m_Mass = 1.0f;
    [SerializeField]
    private float m_Drag = 0.0f;
    [SerializeField]
    private float m_Bounce = 0.0f;
    [SerializeField]
    private Vector2 m_InitialVelocity = Vector2.zero;
    #endregion

    #region Properties
    public Vector2 Velocity => m_Velocity;
    public Vector2 WorldCenterOfMass => m_WorldCenterOfMass;
    public float Mass => m_Mass;

    private Vector2 m_Gravity => new Vector2(0, -9.8f);
    private Vector2 m_WorldCenterOfMass => transform.TransformPoint(m_CenterOfMass);
    private Vector2 m_NextFrameOffset => m_Velocity * TickMachine.DeltaTime;
    #endregion

    #region Fields
    private static Dictionary<List<Collider2D>, PhysicsObject> m_AllPhysicsObjects = new Dictionary<List<Collider2D>, PhysicsObject>();
    private static Dictionary<(PhysicsObject, PhysicsObject), Vector2> m_Intersections = new Dictionary<(PhysicsObject, PhysicsObject), Vector2>();

    private Vector2 m_Velocity;
    private Vector2 m_CenterOfMass;
    private float m_TempDrag = 0.0f;
    private List<Collider2D> m_Colliders = new List<Collider2D>();
    #endregion

    #region Methods

    #region Public

    public static PhysicsObject Find(Collider2D collider)
    {
        foreach (KeyValuePair<List<Collider2D>, PhysicsObject> kvp in m_AllPhysicsObjects)
        {
            if (kvp.Key.Contains(collider)) { return kvp.Value; }
        }
        return null;
    }

    public void OnTick()
    {
        if (!isActiveAndEnabled) { return; }

    }

    public void AddVelocity(Vector2 velocity)
    {
        m_Velocity += velocity;
    }

    public void SetVelocity(Vector2 velocity)
    {
        m_Velocity = velocity;
    }

    public void SetDrag(float drag)
    {
        m_Drag = drag;
    }

    public void ApplyDrag(float drag)
    {
        m_TempDrag = drag;
    }

    public void UpdateCenterOfMass()
    {
        Vector2 cumulativeCenters = Vector2.zero;
        foreach (Collider2D collider in m_Colliders)
        {
            cumulativeCenters += (Vector2)transform.InverseTransformPoint(collider.bounds.center);
        }
        m_CenterOfMass = cumulativeCenters / m_Colliders.Count;
    }

    #endregion

    #region Private
    private void Awake()
    {
        m_Velocity += m_InitialVelocity;
        FetchColliders(transform);
        m_AllPhysicsObjects.Add(m_Colliders, this);
        UpdateCenterOfMass();
        TickMachine.Register(TickOrder.PhysicsObject_FindIntersections, FindIntersections);
        TickMachine.Register(TickOrder.PhysicsObject_HandleIntersections, HandleIntersections);
    }

    private void OnDestroy()
    {
        TickMachine.Unregister(TickOrder.PhysicsObject_FindIntersections, FindIntersections);
        TickMachine.Unregister(TickOrder.PhysicsObject_HandleIntersections, HandleIntersections);
    }

    private void FetchColliders(Transform root)
    {
        Collider2D[] colliders = root.GetComponents<Collider2D>();
        m_Colliders.AddRange(colliders);
        foreach (Transform child in root)
        {
            FetchColliders(child);
        }
    }

    private void SetCollidersActive(bool active)
    {
        foreach (Collider2D collider in m_Colliders)
        {
            collider.enabled = active;
        }
    }

    private void ApplyGravity()
    {
        m_Velocity += m_Gravity * TickMachine.DeltaTime;
    }

    private void ApplyDrag()
    {
        m_Velocity = Vector2.MoveTowards(m_Velocity, Vector2.zero, m_Velocity.magnitude * (m_Drag + m_TempDrag) * TickMachine.DeltaTime);
        m_TempDrag = 0.0f;
    }

    private void FindIntersections()
    {
        if (!isActiveAndEnabled) { return; }
        ApplyGravity();
        EvaluateIntersections();
    }

    private void HandleIntersections()
    {
        if (!isActiveAndEnabled) { return; }

        SetCollidersActive(true);
        ApplyVelocity();
        ApplyDrag();
    }

    private void EvaluateIntersections()
    {
        SetCollidersActive(false);
        //DebugExtension.DebugColliders(m_Colliders, Vector2.zero, Color.yellow); // Current Position
        //DebugExtension.DebugColliders(m_Colliders, m_NextFrameOffset, Color.red); // Next Frame Position
        Dictionary<PhysicsObject, List<Vector2>> positionDeltas = new Dictionary<PhysicsObject, List<Vector2>>();
        Dictionary<PhysicsObject, List<Vector2>> velocityDeltas = new Dictionary<PhysicsObject, List<Vector2>>();

        foreach (Collider2D collider in m_Colliders)
        {
            if (collider is BoxCollider2D)
            {
                BoxCollider2D boxCollider = (BoxCollider2D)collider;
                RaycastHit2D[] hits = Physics2D.BoxCastAll(boxCollider.bounds.center, boxCollider.size, transform.eulerAngles.z, m_NextFrameOffset, m_NextFrameOffset.magnitude);
                foreach (RaycastHit2D hit in hits)
                {
                    Vector2 localHitPoint = hit.point - (Vector2)boxCollider.bounds.center;
                    float xDist = (boxCollider.size.x * 0.5f) - Mathf.Abs(localHitPoint.x);
                    float yDist = (boxCollider.size.y * 0.5f) - Mathf.Abs(localHitPoint.y);
                    DebugExtension.DebugArrow(boxCollider.bounds.center, localHitPoint, Color.green, 1.0f);
                    //Vector2 hitDirection = new Vector2
                    //{
                    //    x = xDist <= yDist ? 1 * Mathf.Sign(localHitPoint.x) : 0,
                    //    y = yDist <= xDist ? 1 * Mathf.Sign(localHitPoint.y) : 0,
                    //};
                    //DebugExtension.DebugArrow(m_WorldCenterOfMass, hitDirection * 0.1f, Color.blue);

                    //SetCollidersActive(true);
                    //RaycastHit2D selfHit = Physics2D.Raycast(hit.point, -hitDirection, m_Velocity.magnitude * TickMachine.DeltaTime, m_SelfMask);
                    //SetCollidersActive(false);
                    //if (selfHit.collider)
                    //{
                    //    //DebugExtension.DebugPoint(selfHit.point, Color.green, 0.01f);
                    //    RaycastHit2D otherHit = Physics2D.Raycast(selfHit.point, hitDirection, m_Velocity.magnitude * TickMachine.DeltaTime);
                    //    if (otherHit.collider)
                    //    {
                    //        //DebugExtension.DebugPoint(otherHit.point, Color.red, 0.01f);
                    //        PhysicsObject hitPhysicsObject = otherHit.collider.GetComponent<PhysicsObject>();
                    //        if (hitPhysicsObject != null)
                    //        {
                    //            Vector2 impact = m_Velocity * m_Mass;
                    //            if (!impactDictionary.ContainsKey(hitPhysicsObject)) { impactDictionary.Add(hitPhysicsObject, impact); }
                    //            else { impactDictionary[hitPhysicsObject] = Vector2.Max(impactDictionary[hitPhysicsObject], impact); }
                    //        }
                    //        HandleIntersection(collider, selfHit.point, otherHit.point, hitDirection);
                    //    }
                    //}
                }
                continue;
            }
            if (collider is CircleCollider2D)
            {
                CircleCollider2D circleCollider = (CircleCollider2D)collider;

                // Check for intersections
                RaycastHit2D[] hits = Physics2D.CircleCastAll(circleCollider.bounds.center, circleCollider.radius, m_NextFrameOffset, m_NextFrameOffset.magnitude);
                foreach (RaycastHit2D hit in hits)
                {
                    Vector2 hitOffset = hit.point - (Vector2)circleCollider.bounds.center;
                    float moveDistance = hitOffset.magnitude - circleCollider.radius;
                    DebugExtension.DebugArrow((Vector2)circleCollider.bounds.center, hitOffset.normalized * moveDistance, Color.green, 1.0f);

                    PhysicsObject other = PhysicsObject.Find(hit.collider);
                    Vector2 hitDirectionVelocity = hitOffset.normalized * Vector2.Dot(hitOffset.normalized, m_Velocity);

                    if (other != null)
                    {
                        Vector2 selfMomentum = m_Velocity * m_Mass;
                        Vector2 otherMomentum = other.Velocity * other.Mass;
                        float combinedMass = m_Mass + other.Mass;
                        Vector2 finalVelocity = (selfMomentum + otherMomentum) / combinedMass;

                        float combinedBounce = (m_Bounce * other.m_Bounce) / 2.0f;
                        if (combinedBounce != 0.0f)
                        {
                            float selfBounceRatio = m_Bounce / combinedBounce;
                            float otherBounceRatio = other.m_Bounce / combinedBounce;
                            Vector2 bounceVelocity = (hitOffset.normalized * finalVelocity.magnitude) * combinedBounce;
                            finalVelocity -= bounceVelocity;

                            AddVelocityDelta(this, -bounceVelocity * selfBounceRatio);
                            AddVelocityDelta(other, bounceVelocity * otherBounceRatio);
                        }

                        AddPositionDelta(this, hitOffset.normalized * moveDistance * 0.5f);
                        AddPositionDelta(other, hitOffset.normalized * -moveDistance * 0.5f);

                        AddVelocityDelta(this, finalVelocity - m_Velocity);
                        AddVelocityDelta(other, finalVelocity - other.Velocity);
                    }
                    else
                    {
                        // Move to edge of collision
                        AddPositionDelta(this, hitOffset.normalized * moveDistance);
                        // Remove velocity in direction of collision
                        AddVelocityDelta(this, -hitDirectionVelocity * (1 + (m_Bounce / 2.0f)));
                    }
                }
            }
        }

        void AddPositionDelta(PhysicsObject target, Vector2 delta)
        {
            if (!positionDeltas.ContainsKey(target)) { positionDeltas.Add(target, new List<Vector2>()); }
            positionDeltas[target].Add(delta);
        }
        void AddVelocityDelta(PhysicsObject target, Vector2 delta)
        {
            if (!velocityDeltas.ContainsKey(target)) { velocityDeltas.Add(target, new List<Vector2>()); }
            velocityDeltas[target].Add(delta);
        }

        // Add all position deltas
        foreach (KeyValuePair<PhysicsObject, List<Vector2>> kvp in positionDeltas)
        {
            PhysicsObject target = kvp.Key;
            List<Vector2> deltas = kvp.Value;
            foreach (Vector2 delta in deltas)
            {
                target.transform.position += (Vector3)delta;
            }
        }

        // Add average of all velocity deltas
        foreach (KeyValuePair<PhysicsObject, List<Vector2>> kvp in velocityDeltas)
        {
            PhysicsObject target = kvp.Key;
            List<Vector2> deltas = kvp.Value;
            Vector2 averageDelta = Vector2.zero;
            foreach (Vector2 delta in deltas)
            {
                averageDelta += delta;
            }
            averageDelta /= deltas.Count;
            target.AddVelocity(averageDelta);
        }

        //void HandleIntersection(Collider2D collider, Vector2 selfHitPoint, Vector2 otherHitPoint, Vector2 hitDirection)
        //{
        //    DebugExtension.DebugPoint(selfHitPoint, Color.yellow, 0.01f);
        //    DebugExtension.DebugPoint(otherHitPoint, Color.red, 0.01f);

        //    Vector2 localOtherHitPoint = otherHitPoint - (Vector2)collider.bounds.center;
        //    Vector2 overlap = Vector2.zero;
        //    if (collider is BoxCollider2D)
        //    {
        //        BoxCollider2D boxCollider = (BoxCollider2D)collider;
        //        if (Mathf.Abs(localOtherHitPoint.x) < boxCollider.size.x * 0.5f && Mathf.Abs(localOtherHitPoint.y) < boxCollider.size.y * 0.5f)
        //        {
        //            float xDist = (boxCollider.size.x * 0.5f) - Mathf.Abs(localOtherHitPoint.x);
        //            float yDist = (boxCollider.size.y * 0.5f) - Mathf.Abs(localOtherHitPoint.y);
        //            overlap = new Vector2
        //            {
        //                x = xDist <= yDist ? xDist * hitDirection.x * 2 : 0,
        //                y = yDist <= xDist ? yDist * hitDirection.y * 2 : 0,
        //            };
        //        }

        //        Vector2 offset = overlap == Vector2.zero ? otherHitPoint - selfHitPoint : -overlap;
        //        transform.position += (Vector3)offset;

        //        // Right Hit
        //        if (hitDirection.x == 1) { m_Velocity.x = Mathf.Min(m_Velocity.x, 0); }
        //        // Left Hit
        //        if (hitDirection.x == -1) { m_Velocity.x = Mathf.Max(0, m_Velocity.x); }
        //        // Top Hit
        //        if (hitDirection.y == 1) { m_Velocity.y = Mathf.Min(m_Velocity.y, 0); }
        //        // Bottom Hit
        //        if (hitDirection.y == -1) { m_Velocity.y = Mathf.Max(0, m_Velocity.y); }
        //    }
        //    if (collider is CircleCollider2D)
        //    {
        //        CircleCollider2D circleCollider = (CircleCollider2D)collider;
        //        float overlapDistance = (circleCollider.radius - localOtherHitPoint.magnitude);
        //        Vector2 offset = Vector2.zero;
        //        if (overlapDistance >= 0.0f)
        //        {
        //            offset = -hitDirection * (circleCollider.radius - localOtherHitPoint.magnitude);
        //        }
        //        else
        //        {
        //            offset = otherHitPoint - selfHitPoint;
        //        }
        //        transform.position += (Vector3)offset;
        //        m_Velocity -= (hitDirection * Vector2.Dot(m_Velocity, hitDirection)) * 0.9f;
        //    }
        //}

        //foreach (KeyValuePair<PhysicsObject, Vector2> pair in impactDictionary)
        //{
        //    pair.Key.AddForce(pair.Value);
        //    Vector2 momentum = m_Velocity * m_Mass;
        //    Vector2 otherMomentum = pair.Key.Velocity * pair.Key.Mass;
        //    m_Velocity = ((momentum + otherMomentum) / (m_Mass + pair.Key.Mass)) * 0.5f;
        //}

        //if (impactDictionary.Count != 0 && m_UnitCollider != null)
        //{
        //    m_UnitCollider.OnPhysicsHit();
        //}

        //SetCollidersActive(true);
    }

    private void ApplyVelocity()
    {
        transform.position += (Vector3)m_Velocity * TickMachine.DeltaTime;
    }

    #endregion

    #endregion

}
