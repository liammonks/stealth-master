using DarkRift;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : SimulationBehaviour
{
    public UnitInput Input => m_Input;
    public UnitAnimator Animator => m_Animator;
    public UnitPhysics Physics => m_Physics;
    public UnitSettings Settings => m_Settings;
    public Spring GroundSpring => m_GroundSpring;
    public Spring WallSpring => m_WallSpring;
    public StateMachine StateMachine => m_StateMachine;
    public UnitCollider Collider => m_Collider;
    public BodyState BodyState => m_BodyState;
    
    public bool UpdateFacingDirection { get { return m_UpdateFacingDirection; } set { m_UpdateFacingDirection = value; } }
    public bool FacingRight { get { return m_FacingRight; } set { m_FacingRight = value; } }

    public Action<BodyState, float> OnBodyStateChanged;

    #region Components
    [SerializeField] private UnitSettings m_Settings;
    [SerializeField] private List<GameObject> m_GibPrefabs;

    private UnitInput m_Input;
    private UnitPhysics m_Physics;
    private Spring m_GroundSpring;
    private Spring m_WallSpring;
    private UnitAnimator m_Animator;
    private UnitCollider m_Collider;
    private StateMachine m_StateMachine;
    #endregion

    private bool m_FacingRight = true;
    private bool m_AimingRight = true;
    private bool m_UpdateFacingDirection = true;
    private BodyState m_BodyState;
    private Transform m_SpringParent;

    protected override void Awake() {
        base.Awake();

        m_Input = GetComponent<UnitInput>();
        m_Input.Initialise();

        m_Physics = GetComponent<UnitPhysics>();
        m_Physics.Initialise();

        m_Animator = GetComponentInChildren<UnitAnimator>();
        m_Animator.Initialise();

        m_Collider = GetComponentInChildren<UnitCollider>();
        m_Collider.Initialise();

        m_StateMachine = GetComponent<StateMachine>();

        // Setup springs
        m_SpringParent = new GameObject("Springs").transform;
        m_SpringParent.SetParent(transform);
        m_SpringParent.localPosition = Vector3.zero;
        m_SpringParent.localRotation = Quaternion.identity;

        m_GroundSpring = m_SpringParent.gameObject.AddComponent<Spring>();
        m_GroundSpring.Initialise(m_Settings.spring.GetGroundSpring(BodyState), Physics.Rigidbody);

        m_WallSpring = m_SpringParent.gameObject.AddComponent<Spring>();
        m_WallSpring.Initialise(m_Settings.spring.GetWallSpring(BodyState), Physics.Rigidbody);
    }

    public override void Simulate(float timeStep)
    {
        UpdateInput();
        UpdateFacing();
        UpdateAiming();
        UpdateAnimator();
        UpdateDrag();
        UpdateStateMachine();
    }

    private void UpdateInput()
    {
        Input.PrepareInput();
    }

    private void UpdateFacing()
    {
        if (m_UpdateFacingDirection)
        {
            if (m_WallSpring.Intersecting)
            {
                // Set facing based on input
                if (m_Input.Movement > 0.0f) { m_FacingRight = true; }
                else if (m_Input.Movement < 0.0f) { m_FacingRight = false; }
            }
            else
            {
                // Set facing based on velocity, then input
                if (m_Physics.Velocity.x > 0.5f) { m_FacingRight = true; }
                else if (m_Physics.Velocity.x < -0.5f) { m_FacingRight = false; }
                else if (m_Input.Movement > 0.0f) { m_FacingRight = true; }
                else if (m_Input.Movement < 0.0f) { m_FacingRight = false; }
            }
        }
        m_SpringParent.localScale = new Vector3(m_FacingRight ? 1 : -1, 1, 1);
        m_Collider.transform.localScale = new Vector3(m_FacingRight ? 1 : -1, 1, 1);
    }

    private void UpdateAiming()
    {
        if (m_Input.MouseWorldPosition.x > 0.0f) { m_AimingRight = true; }
        else if (m_Input.MouseWorldPosition.x < 0.0f) { m_AimingRight = false; }
    }

    private void UpdateAnimator()
    {
        m_Animator.SetVelocity(m_Physics.Velocity);
        m_Animator.SetFacing(m_FacingRight, m_AimingRight);
    }

    private void UpdateDrag()
    {
        m_Physics.CalculateDrag();
    }

    private void UpdateStateMachine()
    {
        m_StateMachine.Execute();
    }

    public void SetBodyState(BodyState state, float duration)
    {
        if (m_BodyState == state) { return; }
        m_BodyState = state;
        OnBodyStateChanged.Invoke(state, duration);
        m_GroundSpring.UpdateSettings(m_Settings.spring.GetGroundSpring(state), duration);
        m_WallSpring.UpdateSettings(m_Settings.spring.GetWallSpring(state), duration);
    }

    #region Rollback

    private struct UnitData : IDarkRiftSerializable
    {
        public bool groundSpringActive;

        public void Deserialize(DeserializeEvent e)
        {
            groundSpringActive = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(groundSpringActive);
        }
    }

    public override List<StateData> GetSimulationState()
    {
        List<StateData> data = new List<StateData>();
        UnitData unitData = new UnitData();
        unitData.groundSpringActive = m_GroundSpring.enabled;

        data.Add(new StateData(this, unitData));
        data.AddRange(Physics.GetSimulationState());
        data.AddRange(StateMachine.GetSimulationState());
        return data;
    }

    public override void SetSimulationState(IDarkRiftSerializable data)
    {
        m_GroundSpring.enabled = ((UnitData)data).groundSpringActive;
    }

    #endregion
}
