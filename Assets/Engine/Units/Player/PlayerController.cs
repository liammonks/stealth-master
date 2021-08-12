using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Unit
{

    private void OnMovement(InputValue value)
    {
        SetMovement(Mathf.RoundToInt(value.Get<Vector2>().x));
    }
    
    private void OnRun(InputValue value)
    {
        SetRunning(value.Get<float>() == 1.0f);
    }
    
    private void OnJump(InputValue value)
    {
        if(value.Get<float>() == 1.0f)
        {
            Jump();
        }
    }
}
