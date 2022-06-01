using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    public enum UnitLayer
    {
        Body,
        Arm
    }

    [Header("Body")]
    [SerializeField] private Animator m_Body;
    [SerializeField] private RuntimeAnimatorController m_DefaultBodyController;
    [SerializeField] private RuntimeAnimatorController m_DefaultBodyControllerRV;

    [SerializeField]
    private Animator m_Arm;
    [SerializeField] private RuntimeAnimatorController m_DefaultArmController;
    [SerializeField] private RuntimeAnimatorController m_DefaultArmControllerRV;

    public void Play(UnitLayer layer, string state)
    {
        GetAnimator(layer).Play(state);
        // If playing animation on the body and arm controller is default, play the animation on the arm too
        if (layer == UnitLayer.Body && LayerIsDefaultController(UnitLayer.Arm))
        {
            GetAnimator(UnitLayer.Arm).Play(state);
        }
    }

    public void SetController(UnitLayer layer, RuntimeAnimatorController controller)
    {
        GetAnimator(layer).runtimeAnimatorController = controller;
    }

    public void SetVelocity(float velocity)
    {
        m_Body.SetFloat(0, velocity);
        m_Arm.SetFloat(0, velocity);
    }

    private Animator GetAnimator(UnitLayer layer)
    {
        switch(layer)
        {
            case UnitLayer.Body: return m_Body;
            case UnitLayer.Arm: return m_Arm;
            default: return null;
        }
    }

    private bool LayerIsDefaultController(UnitLayer layer)
    {
        switch(layer)
        {
            case UnitLayer.Body:
                return m_Body.runtimeAnimatorController == m_DefaultArmController || 
                        m_Body.runtimeAnimatorController == m_DefaultArmControllerRV;
            case UnitLayer.Arm:
                return m_Arm.runtimeAnimatorController == m_DefaultArmController ||
                        m_Arm.runtimeAnimatorController == m_DefaultArmControllerRV;
            default: return false;
        }
    }
}
