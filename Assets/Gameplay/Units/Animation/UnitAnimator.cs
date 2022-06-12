using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{

    public UnitAnimationState CurrentState => m_CurrentState;
    public float CurrentStateLength => m_Body.GetCurrentAnimatorStateInfo(0).length;

    [SerializeField] private UnitAnimations m_Animations;

    [Header("Components")]
    [SerializeField] private Animator m_Body;
    [SerializeField] private Animator m_Arm;

    #region Testing

    [Space(20)] [BoxGroup("Play Animations")] [SerializeField] [OnValueChanged("OnTestAnimationChanged")]
    private UnitAnimationState m_TestState = UnitAnimationState.Idle;

    private void OnTestAnimationChanged()
    {
        Play(m_TestState);
    }

    [PropertySpace(10)] [BoxGroup("Play Animations")] [Button]
    private void ResetAnimation()
    {
        Play(m_TestState, 0.0f);
    }

    [ButtonGroup("Play Animations/Buttons")]
    private void PreviousFrame()
    {
        m_Body.Update(-(1.0f / 12.0f));
        m_Arm.Update(-(1.0f / 12.0f));
    }

    [ButtonGroup("Play Animations/Buttons")]
    private void NextFrame()
    {
        m_Body.Update(1.0f / 12.0f);
        m_Arm.Update(1.0f / 12.0f);
    }

    private void OnValidate()
    {
        if (m_Body == null || m_Arm == null || m_Animations == null) { return; }
        if (!m_Body.gameObject.activeInHierarchy || !m_Arm.gameObject.activeInHierarchy) { return; }
        Play(m_TestState);
    }

    #endregion

    private UnitLayerControllers m_BodyControllers;
    private UnitLayerControllers m_ArmControllers;

    private bool m_AimingForward = false;
    private UnitAnimationState m_CurrentState = UnitAnimationState.Idle;

    private void Awake()
    {
        m_BodyControllers = m_Animations.Body;
        m_ArmControllers = m_Animations.Arm.Unarmed;
    }

    public void Play(UnitAnimationState state, float time = -1.0f)
    {
        m_CurrentState = state;
        if (time == -1.0f) m_Body.Play(state.ToString());
        else m_Body.Play(state.ToString(), 0, time);
        m_Body.Update(0);

        // If arm controller is default, play the animation on the arm too
        if (ArmIsDefaultController())
        {
            if (time == -1.0f) m_Arm.Play(state.ToString());
            else m_Arm.Play(state.ToString(), 0, time);
            m_Arm.Update(0);
        }
    }

    public void Play(GadgetAnimationState state)
    {
        m_Arm.Play(state.ToString());
    }

    public void SetVelocity(Vector2 velocity)
    {
        m_Body.SetFloat("VelocityX", Mathf.Abs(velocity.x));
        m_Arm.SetFloat("VelocityX", Mathf.Abs(velocity.x));
    }

    public void SetFacing(bool facingRight, bool aimingRight)
    {
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);

        bool lastAimingForward = m_AimingForward;
        m_AimingForward = facingRight == aimingRight;
        // Switch controllers if facing has changed
        if (m_AimingForward != lastAimingForward)
        {
            int stateNameHash = m_Body.GetCurrentAnimatorStateInfo(0).shortNameHash;
            float normalizedTime = m_Body.GetCurrentAnimatorStateInfo(0).normalizedTime;
            m_Body.runtimeAnimatorController = m_AimingForward ? m_BodyControllers.Default : m_BodyControllers.Reversed;
            m_Body.Play(stateNameHash, 0, normalizedTime);

            stateNameHash = m_Arm.GetCurrentAnimatorStateInfo(0).shortNameHash;
            normalizedTime = m_Arm.GetCurrentAnimatorStateInfo(0).normalizedTime;
            m_Arm.runtimeAnimatorController = m_AimingForward ? m_ArmControllers.Default : m_ArmControllers.Reversed;
            m_Arm.Play(stateNameHash, 0, normalizedTime);
        }
    }

    private bool ArmIsDefaultController()
    {
        return m_Arm.runtimeAnimatorController == m_Animations.Arm.Unarmed.Default ||
                m_Arm.runtimeAnimatorController == m_Animations.Arm.Unarmed.Reversed;
    }
}
