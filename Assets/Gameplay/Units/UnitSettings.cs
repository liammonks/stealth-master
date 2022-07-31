using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "stealth-master/Unit/Settings")]
public class UnitSettings : ScriptableObject
{
    [Header("Speed")]
    [TabGroup("Stats")] public float runSpeed;
    [TabGroup("Stats")] public float walkSpeed;

    [Header("Acceleration")]
    [TabGroup("Stats")] public float groundAcceleration;
    [TabGroup("Stats")] public float airAcceleration;

    [Header("Jumping")]
    [TabGroup("Stats")] public float jumpForce;
    [TabGroup("Stats")] public Vector2 wallJumpForce;

    [Header("Sliding")]
    [TabGroup("Stats")] public float slideVelocityMultiplier;

    [Header("Vaulting")]
    [TabGroup("Stats")] public float vaultCheckDistance;
    [TabGroup("Stats")] public float vaultCheckMinHeight;
    [TabGroup("Stats")] public float vaultCheckMaxHeight;
    [TabGroup("Stats")] public float vaultOverDistance;

    [Header("Climbing")]
    [TabGroup("Stats")] public Vector2 climbGrabOffset;
    [TabGroup("Stats")] public Vector2 climbRequiredInset;
    [TabGroup("Stats")] public float climbCheckMinHeight;
    [TabGroup("Stats")] public float climbCheckMaxHeight;

    [Header("Drag")]
    [TabGroup("Stats")] public float groundDrag;
    [TabGroup("Stats")] public float airDrag;
    [TabGroup("Stats")] public float slideDrag;

    [Header("Animation")]
    [TabGroup("Components")] public UnitAnimations animations;

    [Header("Springs")]
    [TabGroup("Components")] public UnitSpringSettings spring;
}
