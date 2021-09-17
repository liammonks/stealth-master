using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitData
{
    [HideInInspector] public Animator animator;
    public UnitState possibleStates;
    
    public InputData input = new InputData();
    public UnitStats stats;
    public UnitCollider collider;
    public UnitCollision collision;
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 target;
    public bool isGrounded { get { return (collision & UnitCollision.Ground) != 0; } }
    public bool canStand = true;
    public float t = 0.0f;
    public UnitState previousState;

    public bool ShouldJump()
    {
        bool jumpRequested = Time.unscaledTime - input.jumpRequestTime < 0.1f;
        bool grounded = (collision & UnitCollision.Ground) != 0;
        input.jumpQueued = jumpRequested && grounded;
        return input.jumpQueued;
    }

}

[Serializable]
public class InputData
{
    public int movement;
    public bool jumpQueued;
    public float jumpRequestTime;
    public bool crawling;
    public bool running;
}

[Flags]
public enum UnitCollision
{
    None    = 0,
    Ground  = 1,
    Left    = 2,
    Right   = 4,
    Ceil    = 8
}

[Flags]
public enum UnitState
{
    Null            = 0,
    Idle            = 1,
    Run             = 2,
    Crawl           = 4,
    Slide           = 8,
    Dive            = 16,
    Jump            = 32,
    VaultOverState  = 64,
    VaultOnState  = 128
}


public class Unit : MonoBehaviour
{

    protected LayerMask collisionMask;
    protected LayerMask interactionMask;

    [Header("Components")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator animator;

    [Header("Data")]
    [SerializeField] private UnitState possibleStates;
    [SerializeField] private UnitState state;
    public UnitData data;

    private void Awake() {
        collisionMask = LayerMask.GetMask("UnitCollider");
        interactionMask = LayerMask.GetMask("Interactable");
        data.animator = animator;
        data.possibleStates = possibleStates;
        data.collider.SetStanding();
        UnitStates.Initialise(data, state);
    }

    private void FixedUpdate() 
    {
        UpdateMovement();
        UpdateCollisions();
    }
    
    private void UpdateMovement()
    {
        // Clear velocity if approximatley zero
        if (data.velocity.sqrMagnitude < 0.02f)
        {
            data.velocity = Vector2.zero;
        }

        // Execute movement, recieve next state
        UnitState nextState = UnitStates.Execute(data, state);
        // State Updated
        if (state != nextState)
        {
            // Initialise new state
            data.previousState = state;
            UnitStates.Initialise(data, nextState);
            state = nextState;
        }

        // Collisions clamp velocity
        if((data.collision & UnitCollision.Left) != 0)
        {
            data.velocity.x = Mathf.Max(0, data.velocity.x);
        }
        if ((data.collision & UnitCollision.Right) != 0)
        {
            data.velocity.x = Mathf.Min(0, data.velocity.x);
        }
        if ((data.collision & UnitCollision.Ground) != 0)
        {
            data.velocity.y = Mathf.Max(0, data.velocity.y);
        }
        if ((data.collision & UnitCollision.Ceil) != 0)
        {
            data.velocity.y = Mathf.Min(0, data.velocity.y);
        }

        // Move
        transform.Translate(data.velocity * Time.fixedDeltaTime, Space.World);
        data.position = transform.position;
        
        // Apply Drag
        float drag = data.isGrounded ? data.stats.groundDrag : data.stats.airDrag;
        if (data.isGrounded)
        {
            if (state == UnitState.Slide)
            {
                drag *= 0.1f;
                UnitHelper.Instance.EmitGroundParticles(transform.position + (Vector3.down * data.collider.size.y * 0.5f), data.velocity);
            }
            else if (data.velocity.magnitude > data.stats.runSpeed + 0.1f)
            {
                // Lose traction when moving too fast
                drag *= 0.1f;
                UnitHelper.Instance.EmitGroundParticles(transform.position + (Vector3.down * data.collider.size.y * 0.5f), data.velocity);
            }
        }
        data.velocity = Vector2.Lerp(data.velocity, Vector2.zero, Time.fixedDeltaTime * drag);

        // Terminal Velocity
        data.velocity = Vector2.ClampMagnitude(data.velocity, data.stats.terminalVeloicty);

        // Animate
        animator.SetFloat("VelocityX", data.velocity.x);
        if (data.velocity.x > 1.0f) { animator.SetBool("FacingRight", true); }
        else if (data.velocity.x < -1.0f) { animator.SetBool("FacingRight", false); }
        else if (data.input.movement > 0.0f) { animator.SetBool("FacingRight", true); }
        else if (data.input.movement < 0.0f) { animator.SetBool("FacingRight", false); }
    }

    private void UpdateCollisions()
    {
        data.collider.LerpToTarget();
        
        RaycastHit2D leftFootGrounded, rightFootGrounded, highWallLeft, lowWallLeft, highWallRight, lowWallRight;
        float leftDepth = 0.0f, rightDepth = 0.0f, highDepth = 0.0f, lowDepth = 0.0f;
        float ceilRayDist = Mathf.Min(data.collider.vaultHeight, (data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f));

        #region Ground Check
        // Downwards raycast, left side
        leftFootGrounded = Physics2D.Raycast(transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)), -transform.up, data.collider.vaultHeight, collisionMask);
        Debug.DrawRay(transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)), -transform.up * data.collider.vaultHeight, Color.red);
        if (leftFootGrounded)
        {
            Debug.DrawRay(transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)), -transform.up * leftFootGrounded.distance, Color.green);
            leftDepth = data.collider.vaultHeight - leftFootGrounded.distance;
        }
        // Downwards raycast, right side
        rightFootGrounded = Physics2D.Raycast(transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)), -transform.up, data.collider.vaultHeight, collisionMask);
        Debug.DrawRay(transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)), -transform.up * data.collider.vaultHeight, Color.red);
        if (rightFootGrounded)
        {
            Debug.DrawRay(transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)), -transform.up * rightFootGrounded.distance, Color.green);
            rightDepth = data.collider.vaultHeight - rightFootGrounded.distance;
        }
        if (leftFootGrounded || rightFootGrounded)
        {
            // Player is grounded, vault or move away from ground
            data.collision |= UnitCollision.Ground;
            // Check how high the ground is
            float climbDistance = Mathf.Max(leftDepth, rightDepth);
            if(climbDistance > data.collider.stepHeight && CanVault())
            {
                bool frontFootHit = false;
                if (data.velocity.x > 0 && climbDistance == rightDepth) { frontFootHit = true; }
                if (data.velocity.x < 0 && climbDistance == leftDepth) { frontFootHit = true; }
                Debug.Log(data.velocity.x);
                if (frontFootHit)
                {
                    // Vault when climbing over an object taller than stepHeight
                    bool vaultLeft = leftDepth > rightDepth;
                    Vector2 vaultDirection = vaultLeft ? Vector2.left : Vector2.right;
                    float groundClearance = climbDistance - data.collider.stepHeight;
                    // Get a point half way between the top of the vaultable object and the units feet
                    Vector2 nearCheck = (vaultLeft ? leftFootGrounded : rightFootGrounded).point + (vaultDirection * data.collider.size.x);
                    Vector2 farCheck = (vaultLeft ? leftFootGrounded : rightFootGrounded).point + (vaultDirection * (data.collider.size.x + data.collider.feetSeperation));
                    RaycastHit2D nearHit = Physics2D.Raycast(nearCheck, Vector2.down, groundClearance, collisionMask);
                    RaycastHit2D farHit = Physics2D.Raycast(farCheck, Vector2.down, groundClearance, collisionMask);
                    Debug.DrawLine(nearCheck, nearCheck + (Vector2.down * groundClearance), nearHit ? Color.red : Color.green, 2.0f);
                    Debug.DrawLine(farCheck, farCheck + (Vector2.down * groundClearance), farHit ? Color.red : Color.green, 2.0f);
                    if (nearHit || farHit)
                    {
                        // Object is in landing zone
                        // Climb on to the object
                        state = UnitState.VaultOnState;
                        data.target = (Vector2)transform.position + (Vector2.up * climbDistance) + (vaultDirection * data.collider.feetSeperation);
                        UnitStates.Initialise(data, state);
                    }
                    else
                    {
                        // Landing zone is clear
                        // Climb over the object
                        state = UnitState.VaultOverState;
                        data.target = (Vector2)transform.position + (vaultDirection * (data.collider.size.x + data.collider.feetSeperation));
                        UnitStates.Initialise(data, state);
                    }
                }
                else
                {
                    // Back leg hit vault height, fall over ? 
                    // TRIGGERS AT END OF VAULT
                }
            }
            else
            {
                // Step over the object
                data.velocity.y = (climbDistance * data.collider.stepRate) / Time.fixedDeltaTime;
            }
        }
        else
        {
            if((data.collision & UnitCollision.Ground) != 0)
            {
                // Just left the ground, clear vertical velocity
                data.velocity.y = 0;
            }
            // Player is not grounded, apply gravity
            data.collision &= ~UnitCollision.Ground;
            data.velocity.y -= 9.81f * Time.fixedDeltaTime;
        }
        #endregion

        #region Wall Collision
        // Left side low
        lowWallLeft = Physics2D.Raycast(
            transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)),
            -transform.right,
            (data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f),
            collisionMask
        );
        Debug.DrawRay(
            transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)),
            -transform.right * ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)),
            Color.red
        );
        if (lowWallLeft)
        {
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)),
                -transform.right * lowWallLeft.distance,
                Color.green
            );
            lowDepth = ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)) - lowWallLeft.distance;
        }
        // Left side high
        highWallLeft = Physics2D.Raycast(
            transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
            -transform.right,
            (data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f),
            collisionMask
        );
        Debug.DrawRay(
            transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
            -transform.right * ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)),
            Color.red
        );
        if (highWallLeft)
        {
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist, 0)),
                -transform.right * highWallLeft.distance,
                Color.green
            );
            highDepth = ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)) - highWallLeft.distance;
        }
        if(lowWallLeft || highWallLeft)
        {
            data.collision |= UnitCollision.Left;
            //transform.Translate(new Vector3(Mathf.Max(lowDepth, highDepth), 0, 0));
            data.velocity.x = Mathf.Max(data.velocity.x, data.collider.collisionRate * Mathf.Max(lowDepth, highDepth) * 10);
        }
        else
        {
            data.collision &= ~UnitCollision.Left;
        }

        // Right side low
        lowWallRight = Physics2D.Raycast(
            transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)),
            transform.right,
            (data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f),
            collisionMask
        );
        Debug.DrawRay(
            transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)),
            transform.right * ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)),
            Color.red
        );
        if (lowWallRight)
        {
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight)),
                transform.right * lowWallRight.distance,
                Color.green
            );
            lowDepth = ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)) - lowWallRight.distance;
        }
        // Right side high
        highWallRight = Physics2D.Raycast(
            transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist, 0)),
            transform.right,
            (data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f),
            collisionMask
        );
        Debug.DrawRay(
            transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
            transform.right * ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)),
            Color.red
        );
        if (highWallRight)
        {
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
                transform.right * highWallRight.distance,
                Color.green
            );
            highDepth = ((data.collider.size.x * 0.5f) - (data.collider.feetSeperation * 0.5f)) - highWallRight.distance;
        }
        if(lowWallRight || highWallRight)
        {
            data.collision |= UnitCollision.Right;
            //transform.Translate(new Vector3(-Mathf.Max(lowDepth, highDepth), 0, 0));
            data.velocity.x = Mathf.Min(data.velocity.x, -data.collider.collisionRate * Mathf.Max(lowDepth, highDepth) * 10);
        }
        else
        {
            data.collision &= ~UnitCollision.Right;
        }
        #endregion

        #region Ceiling
        // Left side high
        highWallLeft = Physics2D.Raycast(
            transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
            transform.up,
            ceilRayDist,
            collisionMask
        );
        Debug.DrawRay(
            transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
            transform.up * ceilRayDist,
            Color.red
        );
        if (highWallLeft)
        {
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(-data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist)),
                transform.up * highWallLeft.distance,
                Color.green
            );
            leftDepth = ceilRayDist - highWallLeft.distance;
        }
        
        // Right side high
        highWallRight = Physics2D.Raycast(
            transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist, 0)),
            transform.up,
            ceilRayDist,
            collisionMask
        );
        Debug.DrawRay(
            transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist, 0)),
            transform.up * ceilRayDist,
            Color.red
        );
        if (highWallRight)
        {
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(data.collider.feetSeperation * 0.5f, (data.collider.size.y * 0.5f) - ceilRayDist, 0)),
                transform.up * highWallRight.distance,
                Color.green
            );
            rightDepth = ceilRayDist - highWallLeft.distance;
        }
        
        if(highWallLeft || highWallRight)
        {
            data.collision |= UnitCollision.Ceil;
            transform.Translate(new Vector3(0, -Mathf.Max(leftDepth, rightDepth), 0));
        }
        else
        {
            data.collision &= ~UnitCollision.Ceil;
        }
        
        if(data.collider.size.y == data.collider.standing.size.y) // If crawl collider active
        {
            // Check if player can stand
            // Left side high
            highWallLeft = Physics2D.Raycast(
                transform.TransformPoint(new Vector3(-data.collider.standing.feetSeperation * 0.5f, (data.collider.size.y * 0.5f))),
                transform.up,
                data.collider.size.y,
                collisionMask
            );
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(-data.collider.standing.feetSeperation * 0.5f, (data.collider.size.y * 0.5f))),
                transform.up * data.collider.size.y,
                highWallLeft ? Color.red : Color.clear
            );
            // Right side high
            highWallRight = Physics2D.Raycast(
                transform.TransformPoint(new Vector3(data.collider.standing.feetSeperation * 0.5f, (data.collider.size.y * 0.5f))),
                transform.up,
                data.collider.size.y,
                collisionMask
            );
            Debug.DrawRay(
                transform.TransformPoint(new Vector3(data.collider.standing.feetSeperation * 0.5f, (data.collider.size.y * 0.5f))),
                transform.up * data.collider.size.y,
                highWallRight ? Color.red : Color.clear
            );
            data.canStand = !(highWallLeft || highWallRight);
        }
        else
        {
            data.canStand = true;
        }
        #endregion

        RaycastHit2D centerHit = Physics2D.Linecast(
            transform.position - (transform.up * ((data.collider.size.y * 0.5f) - data.collider.vaultHeight)),
            transform.position + (transform.up * ((data.collider.size.y * 0.5f) - ceilRayDist)),
            collisionMask
        );
        Debug.DrawLine(
            transform.position - (transform.up * ((data.collider.size.y * 0.5f) - data.collider.vaultHeight)),
            transform.position + (transform.up * ((data.collider.size.y * 0.5f) - ceilRayDist)),
            centerHit ? Color.green : Color.red
        );
        if(centerHit)
        {
            // Kill Unit ?
        }
    }

    public InputData GetInputData()
    {
        return data.input;
    }
    
    private bool CanVault()
    {
        if(state == UnitState.Idle || state == UnitState.Run || state == UnitState.Jump)
        {
            // If Vault On or Over is possible
            if((data.possibleStates & (UnitState.VaultOnState | UnitState.VaultOverState)) != 0)
            {
                return true;
            }
        }
        return false;
    }
}