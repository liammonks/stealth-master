using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "stealth-master/UnitSettings")]
public class UnitSettings : ScriptableObject
{
    [Header("Speed")]
    public float runSpeed;
    public float walkSpeed;

    [Header("Acceleration")]
    public float groundAcceleration;
    public float airAcceleration;

    [Header("Jumping")]
    public float jumpForce;
    public Vector2 wallJumpForce;

    [Header("Sliding")]
    public float slideVelocityMultiplier;

    [Header("Vaulting")]
    public float vaultGrabDistance;
    public float vaultMinHeight;
    public float vaultMaxHeight;
    public float vaultOverDistance;

    [Header("Climbing")]
    public float climbGrabDistance;
    public float climbMinHeight;
    public float climbMaxHeight;

    [Header("Drag")]
    public float groundDrag;
    public float airDrag;
    public float slideDrag;
}
