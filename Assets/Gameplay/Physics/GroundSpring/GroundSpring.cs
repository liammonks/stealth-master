using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PhysicsObject))]
public class GroundSpring : MonoBehaviour
{
    [SerializeField]
    private GroundSpringSettings m_Settings;

    public bool Slipping => m_Slipping;
    public bool Grounded => this.enabled ? m_Grounded : false;
    public float GroundDistance => Grounded ? m_GroundDistance : m_CurrentData.distance;
    public GroundSpringSettings.Data Data => m_CurrentData;

    [HideInInspector]
    public PhysicsObject attachedObject;

    private PhysicsObject m_Physics;
    private Unit m_Unit;
    private LayerMask m_EnvironmentMask = 8;
    private bool m_Grounded;
    private bool m_Slipping;
    private float m_GroundDistance;

    [SerializeField]
    private GroundSpringSettings.Data m_CurrentData;
    private Coroutine m_SwitchStateCoroutine;

    private void Awake()
    {
        m_Physics = GetComponent<PhysicsObject>();
        m_Unit = GetComponent<Unit>();
        m_Unit.OnBodyStateChanged += SetState;
        m_CurrentData = new GroundSpringSettings.Data(m_Settings.data[m_Unit.BodyState]);
    }

    private void OnEnable()
    {
        UpdateGroundSpring();
    }

    private void FixedUpdate()
    {
        UpdateGroundSpring();
    }

    private void SetState(BodyState state, float duration)
    {
        if (m_SwitchStateCoroutine != null)
        {
            StopCoroutine(m_SwitchStateCoroutine);
        }
        m_SwitchStateCoroutine = StartCoroutine(InterpolateData(state, duration));
    }

    private IEnumerator InterpolateData(BodyState state, float duration)
    {
        float t = 0.0f;
        GroundSpringSettings.Data previousData = new GroundSpringSettings.Data(m_CurrentData);
        GroundSpringSettings.Data targetData = m_Settings.data[state];
        while (t != 1.0f)
        {
            t = Mathf.Min(t += (Time.deltaTime / duration), 1.0f);

            m_CurrentData.distance = Mathf.Lerp(previousData.distance, targetData.distance, t);
            m_CurrentData.originOffset = Mathf.Lerp(previousData.originOffset, targetData.originOffset, t);
            m_CurrentData.groundReach = Mathf.Lerp(previousData.groundReach, targetData.groundReach, t);
            m_CurrentData.size = Vector2.Lerp(previousData.size, targetData.size, t);
            m_CurrentData.force = Mathf.Lerp(previousData.force, targetData.force, t);
            m_CurrentData.damping = Mathf.Lerp(previousData.damping, targetData.damping, t);
            m_CurrentData.groundedMaxAngle = Mathf.Lerp(previousData.groundedMaxAngle, targetData.groundedMaxAngle, t);
            m_Physics.UpdateCenterOfMass();

            yield return null;
        }
    }

    private void UpdateGroundSpring()
    {
        Vector2 velocity = Vector2.zero;
        Vector2 origin = (Vector2)transform.position + (Vector2.down * Data.originOffset);
        float distance = (Data.distance - Data.originOffset - (Data.size.y * 0.5f)) + Data.groundReach;
        m_Slipping = false;

        RaycastHit2D hit = Physics2D.BoxCast(origin, Data.size, transform.eulerAngles.z, -transform.up, distance, m_EnvironmentMask);
        if (hit)
        {
            DebugExtension.DrawBoxCastOnHit(origin, Data.size * 0.5f, transform.rotation, -transform.up, hit.distance, Color.green);
            Debug.DrawRay(origin, -transform.up * (hit.distance - Data.size.y * 0.5f), Color.green);
            m_GroundDistance = hit.distance + Data.originOffset + (Data.size.y * 0.5f);

            // Apply spring force
            float springDisplacement = distance - hit.distance - Data.groundReach;
            float springForce = springDisplacement * Data.force;
            float springDamp = Vector2.Dot(m_Physics.Velocity, transform.up) * Data.damping;

            velocity = (Vector2)transform.up * (springForce - springDamp) * Time.fixedDeltaTime;

            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            // Check if we are grounded based on angle of surface
            float groundAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (groundAngle > Data.groundedMaxAngle)
            {
                // Surface is not standable, or may have just hit a corner
                // Check for a corner
                Vector2 cornerCheckOrigin = hit.point + (Vector2.up * 0.1f);
                hit = Physics2D.Raycast(cornerCheckOrigin, Vector2.down, 0.15f, m_EnvironmentMask);
                //Debug.DrawRay(cornerCheckOrigin, Vector2.down * 0.6f, Color.red);
                // Raycast does not hit if we are on a corner
                if (hit)
                {
                    m_Slipping = true;
                }
                m_Grounded = false;
            }
            else
            {
                m_Grounded = springDisplacement > -0.1f;
                PhysicsObject hitObject = hit.collider.GetComponent<PhysicsObject>();
                if (hitObject != null) { attachedObject = hitObject; }
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
            DebugExtension.DrawBoxCastOnHit(origin, Data.size * 0.5f, transform.rotation, -transform.up, distance, Color.red);
            Debug.DrawRay(origin, -transform.up * (distance - Data.size.y * 0.5f), Color.red);
            m_Grounded = false;

            // Rotate to default
            //float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            //if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            //float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.stats.airRotationForce)) - (data.stats.airRotationDamping);
            //data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
        }

        m_Physics.AddVelocity(velocity);
    }

}
