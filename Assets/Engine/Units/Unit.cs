using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InputData
{
    public int movement;
    public bool jumpQueued;
    public float jumpRequestTime;
    public bool crawling;
    public bool running;
}

[Serializable]
public class UnitData
{
    public InputData input = new InputData();
    public UnitStats stats;
    public UnitCollider collider;
    public UnitCollision collision;
    public Vector2 velocity;
    public Animator animator;
    public bool isGrounded { get { return (collision & UnitCollision.Ground) != 0; } }
    public float t = 0.0f;
    public string lastStateName;

    public bool ShouldJump()
    {
        bool jumpRequested = Time.unscaledTime - input.jumpRequestTime < 0.1f;
        bool grounded = (collision & UnitCollision.Ground) != 0;
        input.jumpQueued = jumpRequested && grounded;
        return input.jumpQueued;
    }
    
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

public class Unit : MonoBehaviour
{

    protected LayerMask collisionMask;
    protected LayerMask interactionMask;

    [Header("Components")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator animator;

    [Header("Data")]
    [SerializeField] private MoveState moveState;
    public UnitData data;

    private void Awake() {
        collisionMask = LayerMask.GetMask("UnitCollider");
        interactionMask = LayerMask.GetMask("Interactable");
        moveState = moveState.Initialise(data, animator);
    }

    private void FixedUpdate() 
    {
        UpdateMovement();
        UpdateCollisions();
    }
    
    private void UpdateMovement()
    {
        // Clear velocity if approximatley zero
        if (data.velocity.sqrMagnitude < 0.01f)
        {
            data.velocity = Vector2.zero;
        }

        // Execute movement, recieve next state
        MoveState nextMoveState = moveState.Execute(data, animator);
        // State Updated
        if (moveState != nextMoveState)
        {
            // Initialise new state
            data.lastStateName = moveState.name;
            nextMoveState.Initialise(data, animator);
            moveState = nextMoveState;
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

        // Apply Drag
        float drag = data.isGrounded ? data.stats.groundDrag : data.stats.airDrag;
        if (data.isGrounded)
        {
            if (moveState.name == "Slide")
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

        // Animate
        if (data.velocity.x > 0) { animator.SetBool("FacingRight", true); }
        if (data.velocity.x < 0) { animator.SetBool("FacingRight", false); }
        animator.SetFloat("VelocityX", data.velocity.x);
    }

    private void UpdateCollisions()
    {
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
            // Player is grounded, move them out of the ground
            data.collision |= UnitCollision.Ground;
            data.velocity.y = (Mathf.Max(leftDepth, rightDepth) * data.collider.stepRate) / Time.fixedDeltaTime;
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
            transform.Translate(new Vector3(Mathf.Max(lowDepth, highDepth), 0, 0));
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
            transform.Translate(new Vector3(-Mathf.Max(lowDepth, highDepth), 0, 0));
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
            data.collider.vaultHeight,
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
    
}