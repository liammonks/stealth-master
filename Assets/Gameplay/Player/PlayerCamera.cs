using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum PlayerCameraState
{
    Default,
    Inventory
}

public class PlayerCamera : MonoBehaviour
{

    public static PlayerCamera Instance;

    public PlayerCameraState CurrentState => currentState;

    private Animator cameraAnimator;
    private PlayerCameraState currentState;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("Two Instances of PlayerCamera Found");
            return;
        }
        Instance = this;

        cameraAnimator = GetComponent<Animator>();
        SetState(PlayerCameraState.Default);
    }

    public void SetState(PlayerCameraState state)
    {
        cameraAnimator.Play(state.ToString());
        currentState = state;
    }

}
