using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolateFollow : MonoBehaviour
{

    [SerializeField]
    private Transform m_Target;

    private Vector2 m_CurrentTickPosition;
    private Vector2 m_LastTickPosition;
    private Vector2 m_LocalOffset;

    private float m_LerpTime = 0.0f;

    private void Awake()
    {
        m_CurrentTickPosition = (Vector2)m_Target.position + m_LocalOffset;
        m_LastTickPosition = m_CurrentTickPosition;
        m_LocalOffset = transform.localPosition;
    }

    private void FixedUpdate()
    {
        m_LastTickPosition = m_CurrentTickPosition;
        m_CurrentTickPosition = (Vector2)m_Target.position + m_LocalOffset;
        m_LerpTime = 0.0f;
    }

    private void LateUpdate()
    {
        m_LerpTime += Time.deltaTime / TickMachine.DeltaTime;
        transform.position = Vector2.Lerp(m_LastTickPosition, m_CurrentTickPosition, m_LerpTime);
    }
}
