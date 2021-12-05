using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask bulletMask;
    [SerializeField] private TrailRenderer trail;

    private Vector2 m_InitialPosition;
    private Vector2 m_Direction;
    private Vector2 m_ParentVelocity;
    private BulletStats m_Stats;

    public void Fire(Vector2 position, Vector2 direction, Vector2 parentVelocity, BulletStats stats, bool isPlayer)
    {
        bulletMask &= ~(1 << (isPlayer ? 9 : 10)); // Enemy friendly fire
        transform.position = position;
        m_InitialPosition = position;
        m_Direction = direction.normalized;
        m_ParentVelocity = parentVelocity;
        m_Stats = stats;
        trail.Clear();
    }
    
    private void FixedUpdate()
    {
        if(Vector2.Distance(m_InitialPosition, transform.position) >= m_Stats.maxDistance)
        {
            BulletPool.Release(this);
            return;
        }
        Vector2 positionDelta = (m_Direction * m_Stats.speed * Time.fixedDeltaTime) + m_ParentVelocity;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, positionDelta, positionDelta.magnitude, bulletMask);
        Debug.DrawLine(transform.position, transform.position - (Vector3)positionDelta, hit ? Color.green : Color.red, hit ? 2.0f : Time.fixedDeltaTime);
        if(hit.collider != null)
        {
            Unit unit = hit.collider.attachedRigidbody?.GetComponent<Unit>();
            if(unit != null)
            {
                unit.TakeDamage(m_Stats.damage);
            }
            BulletPool.Release(this);
            return;
        }
        transform.position += (Vector3)positionDelta;
    }
}
