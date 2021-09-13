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
    public float lockStateDelay = 0.0f;

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

    private bool executeMoveState = true;

    private void Awake() {
        collisionMask = LayerMask.GetMask("UnitCollider");
        interactionMask = LayerMask.GetMask("Interactable");
        moveState = moveState.Initialise(data, animator);
    }

    private void Update()
    {
        // Clear velocity if approximatley zero
        if(data.velocity.sqrMagnitude < 0.001f)
        {
            data.velocity = Vector2.zero;
        }

        // Execute movement, recieve next state
        if (executeMoveState)
        {
            MoveState nextMoveState = moveState.Execute(data, animator);
            // State Updated
            if (moveState != nextMoveState)
            {
                if (data.lockStateDelay <= 0.0f)
                {
                    // Initialise new state
                    moveState = nextMoveState.Initialise(data, animator);
                }
                else
                {
                    // Lock state execution
                    executeMoveState = false;
                    moveState = nextMoveState;
                }
            }
        }
        else
        {
            data.lockStateDelay -= Time.deltaTime;
            if(data.lockStateDelay <= 0.0f)
            {
                moveState = moveState.Initialise(data, animator);
                executeMoveState = true;
            }
        }

        // Move
        transform.Translate(data.velocity * Time.deltaTime);
        
        // Apply Drag
        float drag = data.isGrounded ? data.stats.groundDrag : data.stats.airDrag;
        // Lose traction when faster than run speed
        if(data.isGrounded && data.velocity.magnitude > data.stats.walkSpeed)
        {
            if (data.input.crawling)
            {
                drag *= 0.1f;
            }
            else
            {
                drag *= 0.2f;
            }
        }
        //data.velocity.x = Mathf.MoveTowards(data.velocity.x, 0.0f, Time.deltaTime * drag);
        data.velocity = Vector2.Lerp(data.velocity, Vector2.zero, Time.deltaTime * drag);
        
        UpdateCollisions();
        
        // Animate
        if (data.velocity.x > 0) { animator.SetBool("FacingRight", true); }
        if (data.velocity.x < 0) { animator.SetBool("FacingRight", false); }
        animator.SetFloat("VelocityX", data.velocity.x);
    }

    private void UpdateCollisions()
    {
        RaycastHit2D leftFootGrounded, rightFootGrounded, highWallLeft, lowWallLeft, highWallRight, lowWallRight;
        float leftFootDepth = 0.0f, rightFootDepth = 0.0f;
        
        #region Ground Check
        // Downwards raycast, left side
        leftFootGrounded = Physics2D.Raycast(transform.position + new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.down, data.collider.vaultHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.down * data.collider.vaultHeight, Color.red);
        if (leftFootGrounded)
        {
            Debug.DrawRay(transform.position + new Vector3(-data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.down * leftFootGrounded.distance, Color.green);
            leftFootDepth = data.collider.vaultHeight - leftFootGrounded.distance;
        }
        // Downwards raycast, right side
        rightFootGrounded = Physics2D.Raycast(transform.position + new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.down, data.collider.vaultHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.down * data.collider.vaultHeight, Color.red);
        if (rightFootGrounded)
        {
            Debug.DrawRay(transform.position + new Vector3(data.collider.feetSeperation * 0.5f, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.down * rightFootGrounded.distance, Color.green);
            rightFootDepth = data.collider.vaultHeight - rightFootGrounded.distance;
        }
        if (leftFootGrounded || rightFootGrounded)
        {
            // Player is grounded, move them out of the ground
            data.collision |= UnitCollision.Ground;
            float climbDistance = Mathf.Max(leftFootDepth, rightFootDepth);
            data.velocity.y = (climbDistance / Time.unscaledDeltaTime) * data.collider.stepRate;
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
            data.velocity.y -= 9.81f * Time.deltaTime;
        }
        #endregion

        #region Wall Collision
        // Left side low
        lowWallLeft = Physics2D.Raycast(transform.position + new Vector3(0, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.left, data.collider.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.left * data.collider.size.x * 0.5f, Color.red);
        if (lowWallLeft)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.left * lowWallLeft.distance, Color.green);
            transform.Translate(new Vector3(((data.collider.size.x * 0.5f) - lowWallLeft.distance), 0));
        }
        // Left side high
        highWallLeft = Physics2D.Raycast(transform.position + new Vector3(0, data.collider.size.y * 0.5f), Vector2.left, data.collider.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, data.collider.size.y * 0.5f), Vector2.left * data.collider.size.x * 0.5f, Color.red);
        if (highWallLeft)
        {
            Debug.DrawRay(transform.position + new Vector3(0, data.collider.size.y * 0.5f), Vector2.left * highWallLeft.distance, Color.green);
            transform.Translate(new Vector3(((data.collider.size.x * 0.5f) - highWallLeft.distance), 0));
        }
        // Right side low
        lowWallRight = Physics2D.Raycast(transform.position + new Vector3(0, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.right, data.collider.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.right * data.collider.size.x * 0.5f, Color.red);
        if (lowWallRight)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(data.collider.size.y * 0.5f) + data.collider.vaultHeight), Vector2.right * lowWallRight.distance, Color.green);
            transform.Translate(new Vector3(-((data.collider.size.x * 0.5f) - lowWallRight.distance), 0));
        }
        // Right side high
        highWallRight = Physics2D.Raycast(transform.position + new Vector3(0, data.collider.size.y * 0.5f), Vector2.right, data.collider.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, data.collider.size.y * 0.5f), Vector2.right * data.collider.size.x * 0.5f, Color.red);
        if (highWallRight)
        {
            Debug.DrawRay(transform.position + new Vector3(0, data.collider.size.y * 0.5f), Vector2.right * highWallRight.distance, Color.green);
            transform.Translate(new Vector3(-((data.collider.size.x * 0.5f) - highWallRight.distance), 0));
        }
        #endregion
    }

    public InputData GetInputData()
    {
        return data.input;
    }
    
}