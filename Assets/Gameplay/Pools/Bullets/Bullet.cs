using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    public delegate void OnHit(RaycastHit2D hit);
    public event OnHit onHit;
    public delegate void OnLost();
    public event OnLost onLost;

    [SerializeField] private LayerMask bulletMask;
    [SerializeField] private TrailRenderer trail;

    private Vector2 m_Velocity;
    private Vector2 m_InitialPosition;
    private float distanceTraveled;
    private BulletStats m_Stats;

    public void Fire(Vector2 position, Vector2 direction, Vector2 parentVelocity, BulletStats stats, bool isPlayer)
    {
        bulletMask = Unit.CollisionMask | (1 << (isPlayer ? 10 : 9));
        m_Stats = stats;
        m_Velocity = (direction.normalized * m_Stats.speed) + parentVelocity;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        transform.position = position;
        m_InitialPosition = position;
        distanceTraveled = 0.0f;
        trail.Clear();
    }
    
    private void FixedUpdate()
    {
        if(distanceTraveled >= m_Stats.range)
        {
            onLost.Invoke();
            BulletPool.Release(this);
            return;
        }
        // Move bullet with velocity, clamping travel distance to range
        Vector2 positionDelta = m_Velocity * Time.fixedDeltaTime;
        distanceTraveled += positionDelta.magnitude;
        float excessDistance = distanceTraveled - m_Stats.range;
        if (excessDistance > 0.0f)
        {
            positionDelta = Vector2.ClampMagnitude(positionDelta, positionDelta.magnitude - excessDistance);
            distanceTraveled = m_Stats.range;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, positionDelta, positionDelta.magnitude, bulletMask);
        if(hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.green, 2.0f);
            BulletHit(hit);
            return;
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position - (Vector3)positionDelta, Color.red, Time.fixedDeltaTime);
        }
        transform.position += (Vector3)positionDelta;
    }
    
    private void BulletHit(RaycastHit2D hit)
    {
        Unit unit = hit.collider.attachedRigidbody?.GetComponent<Unit>();
        if (unit != null) { unit.TakeDamage(m_Stats.damage); }
        onHit?.Invoke(hit);
        BulletPool.Release(this);
    }
}
