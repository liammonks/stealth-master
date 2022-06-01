using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    private Vector2 m_Gravity => new Vector2(0, 9.8f);
    private Vector2 m_NextFrameOffset => m_Velocity * Time.fixedDeltaTime;

    private Vector2 m_Velocity;
    private List<Collider2D> m_Colliders = new List<Collider2D>();

    private void Awake()
    {
        FetchColliders(transform);
    }

    private void FetchColliders(Transform root)
    {
        m_Colliders.AddRange(root.GetComponents<Collider2D>());
        foreach (Transform child in root)
        {
            FetchColliders(child);
        }
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        RaycastHit2D[] intersections = GetIntersections();
        ApplyVelocity();
    }

    private void ApplyGravity()
    {
        m_Velocity += m_Gravity * Time.fixedDeltaTime;
    }

    private void ApplyVelocity()
    {
        transform.position += (Vector3)m_Velocity * Time.fixedDeltaTime;
    }

    private RaycastHit2D[] GetIntersections()
    {
        RaycastHit2D[] intersections = new RaycastHit2D[m_Colliders.Count];
        for (int i = 0; i < m_Colliders.Count; ++i)
        {
            if (m_Colliders[i] is BoxCollider2D)
            {
                BoxCollider2D collider = (BoxCollider2D)m_Colliders[i];
                intersections[i] = Physics2D.BoxCast(collider.bounds.center, collider.size, transform.eulerAngles.z, m_NextFrameOffset, m_NextFrameOffset.magnitude);
                continue;
            }
            if (m_Colliders[i] is CircleCollider2D)
            {
                CircleCollider2D collider = (CircleCollider2D)m_Colliders[i];
                intersections[i] = Physics2D.CircleCast(collider.bounds.center, collider.radius, m_NextFrameOffset, m_NextFrameOffset.magnitude);
                continue;
            }
        }
        return intersections;
    }
}
