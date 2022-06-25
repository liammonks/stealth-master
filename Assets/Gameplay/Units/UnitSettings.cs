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
    public float vaultCheckDistance;
    public float vaultCheckMinHeight;
    public float vaultCheckMaxHeight;
    public float vaultOverDistance;

    [Header("Climbing")]
    public Vector2 climbGrabOffset;
    public float climbCheckDistance;
    public float climbCheckMinHeight;
    public float climbCheckMaxHeight;

    [Header("Drag")]
    public float groundDrag;
    public float airDrag;
    public float slideDrag;
}
