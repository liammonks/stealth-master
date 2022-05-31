using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitInput))]
public class Unit : MonoBehaviour
{
    public enum CollisionState
    {
        Standing,
        Crouching,
        Crawling
    }

    public UnitInput Input => m_Input;
    public UnitAnimator Animator => m_Animator;

    private CollisionState m_CollisionState;
    private UnitInput m_Input;
    private UnitAnimator m_Animator;

    private void Awake() {
        m_Input = GetComponent<UnitInput>();
        m_Animator = GetComponent<UnitAnimator>();
    }
}
