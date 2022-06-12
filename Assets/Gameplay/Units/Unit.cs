using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{

    public UnitInput Input => m_Input;
    public UnitAnimator Animator => m_Animator;
    public PhysicsObject Physics => m_Physics;
    public UnitSettings Settings => m_Settings;
    public GroundSpring GroundSpring => m_GroundSpring;
    public StateMachine StateMachine => m_StateMachine;
    public UnitCollider Collider => m_Collider;
    public BodyState BodyState => m_BodyState;
    public bool FacingRight => m_FacingRight;
    public Vector2 Center => Physics.WorldCenterOfMass;

    public Action<BodyState, float> OnBodyStateChanged;

    #region Components
    [SerializeField] private UnitSettings m_Settings;
    [SerializeField] private List<GameObject> m_GibPrefabs;

    private UnitInput m_Input;
    private PhysicsObject m_Physics;
    private GroundSpring m_GroundSpring;
    private StateMachine m_StateMachine;
    private UnitAnimator m_Animator;
    private UnitCollider m_Collider;
    #endregion

    private bool m_FacingRight;
    private bool m_AimingRight;
    private BodyState m_BodyState;

    private void Awake() {
        m_Input = GetComponent<UnitInput>();
        m_Physics = GetComponent<PhysicsObject>();
        m_GroundSpring = GetComponent<GroundSpring>();
        m_StateMachine = GetComponent<StateMachine>();
        m_Animator = GetComponentInChildren<UnitAnimator>();
        m_Collider = GetComponentInChildren<UnitCollider>();
    }

    private void Update()
    {
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        m_Animator.SetVelocity(m_Physics.Velocity);

        if (m_Physics.Velocity.x > 0.1f) { m_FacingRight = true; }
        else if (m_Physics.Velocity.x < -0.1f) { m_FacingRight = false; }
        else if (m_Input.Movement > 0.0f) { m_FacingRight = true; }
        else if (m_Input.Movement < 0.0f) { m_FacingRight = false; }

        if (m_Input.MouseOffset.x > 0.0f) { m_AimingRight = true; }
        else if (m_Input.MouseOffset.x < 0.0f) { m_AimingRight = false; }

        m_Animator.SetFacing(m_FacingRight, m_AimingRight);
    }

    public void SetBodyState(BodyState state, float duration)
    {
        m_BodyState = state;
        OnBodyStateChanged.Invoke(state, duration);
    }

    public void SpawnGibs(Vector2 position, float force)
    {
        foreach (GameObject gibPrefab in m_GibPrefabs)
        {
            GameObject gib = Instantiate(gibPrefab, position, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
            gib.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized * (force * Random.Range(0.5f, 1.0f));
        }
    }
}
