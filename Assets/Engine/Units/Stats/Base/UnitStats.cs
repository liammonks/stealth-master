using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "stealth-master/UnitStats", order = 0)]
public class UnitStats : ScriptableObject {
    
    [Header("Stats")]
    public float walkSpeed = 3;
    public float runSpeed = 5;
    public float groundAcceleration = 100;
    public float airAcceleration = 20;
    [Space(10)]
    public float groundDrag = 10;
    public float airDrag = 1;
    public float slideDrag = 1;
    [Space(10)]
    public float jumpForce = 6.0f;
    public float diveVelocityMultiplier = 0.75f;
    public float slideVelocityMultiplier = 0.25f;

    [Header("Ground Spring")]
    public float standingSpringDistance = 1.0f;
    public Vector2 standingSpringSize;
    //public float standingSpringWidth = 0.3f;
    [Space(10)]
    public float crawlingSpringDistance = 0.5f;
    //public float crawlingSpringWidth = 0.6f;
    public Vector2 crawlingSpringSize;
    [Space(10)]
    public float springForce = 100.0f;
    public float springDamping = 20.0f;
    [Space(10)]
    public float groundRotationForce = 100.0f;
    public float airRotationForce = 100.0f;
    public float groundedMaxAngle = 25.0f;

    [Header("Vaulting")]
    public float vaultGrabDistance = 0.5f;
    public float vaultMoveDistance = 0.5f;
    public float maxVaultHeight = 1.0f;
    public float minVaultHeight = 0.5f;
    public float vaultDuration = 0.65f;

    [Header("Climbing")]
    public float climbGrabDistance = 0.5f;
    public float climbMoveDistance = 0.5f;
    public float maxClimbHeight = 2.0f;
    public float minClimbHeight = 1.5f;
    public float climbDuration = 0.65f;
    public Vector2 climbGrabOffset;
    public Vector2 wallJumpForce;
    public float wallDetectionDistance = 0.75f;

    [Header("Ceiling")]
    public float standingCeilingCheckHeight = 1.0f;
    public float crawlingCeilingCheckHeight = 1.0f;

}