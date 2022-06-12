using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private float m_Mass = 1.0f;
    [SerializeField]
    private float m_Drag = 0.0f;
    #endregion

    #region Properties
    public Vector2 Velocity => m_Velocity;
    public Vector2 WorldCenterOfMass => m_WorldCenterOfMass;
    public float Mass => m_Mass;

    private Vector2 m_Gravity => new Vector2(0, -9.8f);
    private Vector2 m_WorldCenterOfMass => transform.TransformPoint(m_CenterOfMass);
    private Vector2 m_NextFrameOffset => m_Velocity * Time.fixedDeltaTime;
    #endregion

    #region Fields
    private Vector2 m_Velocity;
    private Vector2 m_CenterOfMass;
    private LayerMask m_SelfMask;
    private float m_TempDrag = 0.0f;
    private List<Collider2D> m_Colliders = new List<Collider2D>();
    #endregion

    #region Methods

    #region Public

    public void AddVelocity(Vector2 velocity)
    {
        m_Velocity += velocity;
    }

    public void SetVelocity(Vector2 velocity)
    {
        m_Velocity = velocity;
    }

    public void AddForce(Vector2 force)
    {
        m_Velocity += force / m_Mass;
        DebugExtension.DebugArrow(m_WorldCenterOfMass, force / m_Mass, Color.magenta, 1.0f);
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
        FetchColliders(transform);
        UpdateCenterOfMass();
    }

    private void FetchColliders(Transform root)
    {
        Collider2D[] colliders = root.GetComponents<Collider2D>();
        m_Colliders.AddRange(colliders);
        if (colliders.Length > 0) { m_SelfMask |= LayerMask.GetMask(LayerMask.LayerToName(root.gameObject.layer)); }
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

    private void FixedUpdate()
    {
        ApplyGravity();
        EvaluateIntersections();
        ApplyVelocity();
        ApplyDrag();
        DebugExtension.DebugColliders(m_Colliders, Vector2.zero, Color.white);
    }

    private void ApplyGravity()
    {
        m_Velocity += m_Gravity * Time.fixedDeltaTime;
    }

    private void ApplyDrag()
    {
        m_Velocity = Vector2.MoveTowards(m_Velocity, Vector2.zero, m_Velocity.magnitude * (m_Drag + m_TempDrag) * Time.fixedDeltaTime);
        m_TempDrag = 0.0f;
    }

    private void EvaluateIntersections()
    {
        SetCollidersActive(false);
        //DebugExtension.DebugColliders(m_Colliders, Vector2.zero, Color.yellow); // Current Position
        //DebugExtension.DebugColliders(m_Colliders, m_NextFrameOffset, Color.red); // Next Frame Position
        Dictionary<PhysicsObject, Vector2> impactDictionary = new Dictionary<PhysicsObject, Vector2>();

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
                    Vector2 hitDirection = new Vector2
                    {
                        x = xDist <= yDist ? 1 * Mathf.Sign(localHitPoint.x) : 0,
                        y = yDist <= xDist ? 1 * Mathf.Sign(localHitPoint.y) : 0,
                    };
                    DebugExtension.DebugArrow(m_WorldCenterOfMass, hitDirection * 0.1f, Color.blue);

                    SetCollidersActive(true);
                    RaycastHit2D selfHit = Physics2D.Raycast(hit.point, -hitDirection, m_Velocity.magnitude * Time.fixedDeltaTime, m_SelfMask);
                    SetCollidersActive(false);
                    if (selfHit.collider)
                    {
                        //DebugExtension.DebugPoint(selfHit.point, Color.green, 0.01f);
                        RaycastHit2D otherHit = Physics2D.Raycast(selfHit.point, hitDirection, m_Velocity.magnitude * Time.fixedDeltaTime);
                        if (otherHit.collider)
                        {
                            //DebugExtension.DebugPoint(otherHit.point, Color.red, 0.01f);
                            PhysicsObject hitPhysicsObject = otherHit.collider.GetComponent<PhysicsObject>();
                            if (hitPhysicsObject != null)
                            { 
                                Vector2 impact = m_Velocity * m_Mass;
                                if (!impactDictionary.ContainsKey(hitPhysicsObject)) { impactDictionary.Add(hitPhysicsObject, impact); }
                                else { impactDictionary[hitPhysicsObject] = Vector2.Max(impactDictionary[hitPhysicsObject], impact); }
                            }
                            HandleIntersection(collider, selfHit.point, otherHit.point, hitDirection);
                        }
                    }
                }
                continue;
            }
            if (collider is CircleCollider2D)
            {
                CircleCollider2D circleCollider = (CircleCollider2D)collider;
                RaycastHit2D[] hits = Physics2D.CircleCastAll(circleCollider.bounds.center, circleCollider.radius, m_NextFrameOffset, m_NextFrameOffset.magnitude);
                foreach (RaycastHit2D hit in hits)
                {
                    Vector2 localHitPoint = hit.point - (Vector2)circleCollider.bounds.center;
                    DebugExtension.DebugArrow(m_WorldCenterOfMass, localHitPoint.normalized * 0.1f, Color.blue);

                    SetCollidersActive(true);
                    RaycastHit2D selfHit = Physics2D.Raycast(hit.point, -localHitPoint.normalized, m_Velocity.magnitude * Time.fixedDeltaTime, m_SelfMask);
                    SetCollidersActive(false);
                    if (selfHit.collider)
                    {
                        //DebugExtension.DebugPoint(selfHit.point, Color.green, 0.01f);
                        RaycastHit2D otherHit = Physics2D.Raycast(selfHit.point, localHitPoint.normalized, m_Velocity.magnitude * Time.fixedDeltaTime);
                        if (otherHit.collider)
                        {
                            //DebugExtension.DebugPoint(otherHit.point, Color.red, 0.01f);
                            PhysicsObject hitPhysicsObject = otherHit.collider.GetComponent<PhysicsObject>();
                            if (hitPhysicsObject != null)
                            {
                                Vector2 impact = m_Velocity * m_Mass;
                                if (!impactDictionary.ContainsKey(hitPhysicsObject)) { impactDictionary.Add(hitPhysicsObject, impact); }
                                else { impactDictionary[hitPhysicsObject] = Vector2.Max(impactDictionary[hitPhysicsObject], impact); }
                            }
                            HandleIntersection(collider, selfHit.point, otherHit.point, localHitPoint.normalized);
                        }
                    }
                }
                continue;
            }
        }

        void HandleIntersection(Collider2D collider, Vector2 selfHitPoint, Vector2 otherHitPoint, Vector2 hitDirection)
        {
            Vector2 localOtherHitPoint = otherHitPoint - (Vector2)collider.bounds.center;
            Vector2 overlap = Vector2.zero;
            if (collider is BoxCollider2D)
            {
                BoxCollider2D boxCollider = (BoxCollider2D)collider;
                if (Mathf.Abs(localOtherHitPoint.x) < boxCollider.size.x * 0.5f && Mathf.Abs(localOtherHitPoint.y) < boxCollider.size.y * 0.5f)
                {
                    float xDist = (boxCollider.size.x * 0.5f) - Mathf.Abs(localOtherHitPoint.x);
                    float yDist = (boxCollider.size.y * 0.5f) - Mathf.Abs(localOtherHitPoint.y);
                    overlap = new Vector2
                    {
                        x = xDist <= yDist ? xDist * hitDirection.x * 2 : 0,
                        y = yDist <= xDist ? yDist * hitDirection.y * 2 : 0,
                    };
                }

                Vector2 offset = overlap == Vector2.zero ? otherHitPoint - selfHitPoint : -overlap;
                transform.position += (Vector3)offset;

                // Right Hit
                if (hitDirection.x == 1) { m_Velocity.x = Mathf.Min(m_Velocity.x, 0); }
                // Left Hit
                if (hitDirection.x == -1) { m_Velocity.x = Mathf.Max(0, m_Velocity.x); }
                // Top Hit
                if (hitDirection.y == 1) { m_Velocity.y = Mathf.Min(m_Velocity.y, 0); }
                // Bottom Hit
                if (hitDirection.y == -1) { m_Velocity.y = Mathf.Max(0, m_Velocity.y); }
            }
            if (collider is CircleCollider2D)
            {
                DebugExtension.DebugPoint(selfHitPoint, Color.yellow, 0.01f);
                DebugExtension.DebugPoint(otherHitPoint, Color.red, 0.01f);
                CircleCollider2D circleCollider = (CircleCollider2D)collider;
                float overlapDistance = (circleCollider.radius - localOtherHitPoint.magnitude);
                Vector2 offset = Vector2.zero;
                if (overlapDistance >= 0.0f)
                {
                    offset = -hitDirection * (circleCollider.radius - localOtherHitPoint.magnitude);
                }
                else
                {
                    offset = otherHitPoint - selfHitPoint;
                }
                transform.position += (Vector3)offset;
                m_Velocity -= hitDirection * Vector2.Dot(m_Velocity, hitDirection);
            }
        }

        foreach (KeyValuePair<PhysicsObject, Vector2> pair in impactDictionary)
        {
            pair.Key.AddForce(pair.Value);
            Vector2 momentum = m_Velocity * m_Mass;
            Vector2 otherMomentum = pair.Key.Velocity * pair.Key.Mass;
            m_Velocity = ((momentum + otherMomentum) / (m_Mass + pair.Key.Mass)) * 0.5f;
        }

        SetCollidersActive(true);
    }

    private void ApplyVelocity()
    {
        transform.position += (Vector3)m_Velocity * Time.fixedDeltaTime;
    }

    #endregion

    #endregion

}
