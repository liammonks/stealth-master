using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{

    [SerializeField] private LayerMask bulletMask;
    [SerializeField] private TrailRenderer trail;

    private Vector2 m_Velocity;
    private float distanceTraveled;
    private BulletStats m_Stats;

    public void Fire(Vector2 position, Vector2 direction, Vector2 parentVelocity, BulletStats stats, bool isPlayer)
    {
        bulletMask = Unit.CollisionMask | (1 << (isPlayer ? 10 : 9));
        m_Stats = stats;
        m_Velocity = (direction.normalized * m_Stats.speed) + parentVelocity;
        transform.position = position;
        distanceTraveled = 0.0f;
        trail.Clear();
    }
    
    private void FixedUpdate()
    {
        if(distanceTraveled >= m_Stats.maxDistance)
        {
            BulletPool.Release(this);
            return;
        }
        Vector2 positionDelta = m_Velocity * Time.fixedDeltaTime;
        distanceTraveled += positionDelta.magnitude;
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
