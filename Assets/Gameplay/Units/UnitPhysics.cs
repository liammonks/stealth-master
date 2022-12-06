using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPhysics : MonoBehaviour
{

    public enum DragState
    {
        None,
        Default,
        Sliding,
    }

    public Rigidbody2D Rigidbody => m_Rigidbody;
    public Vector2 Velocity { get { return m_Rigidbody.velocity; } set { m_Rigidbody.velocity = value; } }
    public Vector2 WorldCenterOfMass => m_Rigidbody.worldCenterOfMass;

    private Rigidbody2D m_Rigidbody;
    private Unit m_Unit;

    private bool m_ShouldOverrideDrag;
    private float m_OverrideDragValue;
    private DragState m_DragState = DragState.Default;

    private float m_Drag { get { return m_Rigidbody.drag; } set { m_Rigidbody.drag = value; } }

    public void Initialise()
    {
        m_Rigidbody = gameObject.GetOrAddComponent<Rigidbody2D>();
        m_Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        m_Unit = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        m_Rigidbody.simulated = true;
    }

    private void OnDisable()
    {
        m_Rigidbody.simulated = false;
    }

    #region Drag

    public void CalculateDrag()
    {
        // Force override drag
        if (m_ShouldOverrideDrag)
        {
            m_Drag = m_OverrideDragValue;
            return;
        }

        // Force air drag if not grounded
        if (!m_Unit.GroundSpring.Intersecting)
        {
            m_Drag = m_Unit.Settings.airDrag;
            return;
        }

        // Decide drag based on state
        switch (m_DragState)
        {
            case DragState.None:
                m_Drag = 0;
                break;
            case DragState.Default:
                m_Drag = m_Unit.GroundSpring.Intersecting ? m_Unit.Settings.groundDrag : m_Unit.Settings.airDrag;
                break;
            case DragState.Sliding:
                m_Drag = m_Unit.Settings.slideDrag;
                break;
            default:
                Debug.LogError("Invalid drag state", this);
                break;
        }
    }

    public void SetDragState(DragState state)
    {
        m_DragState= state;
    }

    public void SkipDrag()
    {
        float previousDrag = m_Drag;
        m_Drag = 0;
        StartCoroutine(EnableDrag());

        IEnumerator EnableDrag()
        {
            yield return new WaitForFixedUpdate();
            m_Drag = previousDrag;
        }
    }

    public void OverrideDrag(float drag)
    {
        m_ShouldOverrideDrag = true;
        m_OverrideDragValue = drag;
    }

    public void ReleaseDrag()
    {
        m_ShouldOverrideDrag = false;
    }

    #endregion

}
