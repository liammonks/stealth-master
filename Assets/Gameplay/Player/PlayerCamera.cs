using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum PlayerCameraState
{
    Player
}

public class PlayerCamera : MonoBehaviour
{

    [SerializeField]
    private CinemachineBrain m_CinemachineBrain;

    [SerializeField]
    private Transform m_CinemachineTarget;

    [SerializeField]
    private Animator m_StateAnimator;

    private Transform m_Target;
    private PlayerCameraState currentState;

    private void Awake()
    {
        SetState(PlayerCameraState.Player);
    }

    private void Update()
    {
        if (m_Target == null) { return; }
        m_CinemachineTarget.position = m_Target.position;
    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
    }

    public void SetState(PlayerCameraState state)
    {
        if (currentState == state) { return; }
        m_StateAnimator.Play(state.ToString());
        currentState = state;
    }

}
