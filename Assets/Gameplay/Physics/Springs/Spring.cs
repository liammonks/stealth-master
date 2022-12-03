using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public bool Slipping => m_Slipping;
    public bool Intersecting => this.enabled ? m_Intersecting : false;
    public float HitDistance => Intersecting ? m_HitDistance : m_Settings.position.magnitude + m_Settings.restDistance;
    public Rigidbody2D AttachedPhysics { get { return m_AttachedPhysics; } set { m_AttachedPhysics = value; } }


    private SpringSettings m_Settings;
    private bool m_Slipping = false;
    private bool m_Intersecting = false;
    private float m_HitDistance = 0;
    private LayerMask m_EnvironmentMask = 8;
    private Rigidbody2D m_Physics;
    private Rigidbody2D m_AttachedPhysics;
    private Coroutine m_UpdateSettingsCoroutine;
    private bool m_Initialised = false;
    private bool m_DrawGizmos = false;

    public void Initialise(SpringSettings settings, Rigidbody2D physics)
    {
        UpdateSettings(settings);
        m_Physics = physics;
        m_Initialised = true;
        TickMachine.Register(TickOrder.Spring, OnTick);
    }

    private void OnEnable()
    {
        UpdateSpring();
    }

    public void OnTick()
    {
        if (!isActiveAndEnabled) { return; }
        UpdateSpring();
    }

    private void OnDestroy()
    {
        TickMachine.Unregister(TickOrder.Spring, OnTick);
    }

    public void UpdateSettings(SpringSettings newSettings, float interpDuration = 0.0f)
    {
        if (interpDuration == 0.0f)
        {
            m_Settings = newSettings.Clone();
        }
        else
        {
            if (m_UpdateSettingsCoroutine != null)
            {
                StopCoroutine(m_UpdateSettingsCoroutine);
            }
            m_UpdateSettingsCoroutine = StartCoroutine(UpdateSettingsCoroutine());
        }

        IEnumerator UpdateSettingsCoroutine()
        {
            SpringSettings originalSettings = m_Settings.Clone();
            float t = Time.deltaTime / interpDuration;
            do
            {
                m_Settings.position = Vector2.Lerp(originalSettings.position, newSettings.position, t);
                m_Settings.direction = Vector2.Lerp(originalSettings.direction, newSettings.direction, t);
                m_Settings.size = Vector2.Lerp(originalSettings.size, newSettings.size, t);
                m_Settings.restDistance = Mathf.Lerp(originalSettings.restDistance, newSettings.restDistance, t);
                m_Settings.reachDistance = Mathf.Lerp(originalSettings.reachDistance, newSettings.reachDistance, t);
                m_Settings.force = Mathf.Lerp(originalSettings.force, newSettings.force, t);
                m_Settings.damping = Mathf.Lerp(originalSettings.damping, newSettings.damping, t);
                m_Settings.slipAngle = Mathf.Lerp(originalSettings.slipAngle, newSettings.slipAngle, t);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime / interpDuration;
            }
            while (t < 1.0f);
        }
    }

    private void UpdateSpring()
    {
        if(!m_Initialised) { return; }
        Vector2 velocity = Vector2.zero;
        Vector2 origin = transform.TransformPoint(m_Settings.position);
        Vector2 direction = transform.TransformVector(m_Settings.direction).normalized;
        float sizeInDirection = Mathf.Abs(Vector2.Dot(direction, m_Settings.size));
        float distance = m_Settings.restDistance + m_Settings.reachDistance;
        m_Slipping = false;

        RaycastHit2D hit = Physics2D.BoxCast(origin, m_Settings.size, transform.eulerAngles.z, direction, distance, m_EnvironmentMask);
        if (hit)
        {
            m_HitDistance = Vector2.Dot((hit.point - (Vector2)transform.position), direction);

            if (m_DrawGizmos)
            {
                DebugExtension.DrawBoxCastOnHit(origin, m_Settings.size * 0.5f, transform.rotation, direction, hit.distance, Color.green);
                Debug.DrawRay(origin, direction * (Vector2.Dot((hit.point - origin), direction) - sizeInDirection), Color.green);
            }

            // Apply spring force
            float springDisplacement = distance - hit.distance - m_Settings.reachDistance;
            float springForce = springDisplacement * m_Settings.force;
            float springDamp = Vector2.Dot(m_Physics.velocity, -direction) * m_Settings.damping;

            velocity = -direction * (springForce - springDamp) * Time.fixedDeltaTime;

            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            // Check if we are grounded based on angle of surface
            float hitAngle = Vector2.Angle(hit.normal, -direction);
            if (hitAngle > m_Settings.slipAngle)
            {
                // Surface is not standable, or may have just hit a corner
                // Check for a corner
                //Vector2 cornerCheckOrigin = hit.point + (-direction * 0.1f);
                //hit = Physics2D.Raycast(cornerCheckOrigin, direction, 0.15f, m_EnvironmentMask);

                m_Slipping = true;
                m_Intersecting = false;
            }
            else
            {
                m_Intersecting = springDisplacement > -0.1f;
                Rigidbody2D hitObject = hit.collider.GetComponent<Rigidbody2D>();
                if (hitObject != null) { m_AttachedPhysics = hitObject; }
            }

            // Rotate Unit
            //float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            //if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            //rotationDisplacement -= Vector2.SignedAngle(Vector2.up, hit.normal);
            //float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.isGrounded ? data.stats.groundRotationForce : data.stats.airRotationForce)) - (data.isGrounded ? data.stats.groundRotationDamping : data.stats.airRotationDamping);
            //data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
        }
        else
        {
            if (m_DrawGizmos)
            {
                DebugExtension.DrawBoxCastOnHit(origin, m_Settings.size * 0.5f, transform.rotation, direction, distance, Color.red);
                Debug.DrawRay(origin, direction * (distance - (sizeInDirection * 0.5f)), Color.red);
                Debug.DrawRay(origin, direction * ((distance - m_Settings.reachDistance) - (sizeInDirection * 0.5f)), Color.yellow);
            }

            m_Intersecting = false;

            // Rotate to default
            //float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            //if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            //float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.stats.airRotationForce)) - (data.stats.airRotationDamping);
            //data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
        }

        m_Physics.velocity += velocity;
        m_DrawGizmos = false;
    }

    private void OnDrawGizmos()
    {
        m_DrawGizmos = true;
    }
}
