using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InputData
{
    public int movement;
    public bool jumpQueued;
    public bool crawlQueued;
    public bool running;
}

[Serializable]
public class UnitData
{
    public InputData input = new InputData();
    public UnitStats stats;
    public UnitCollision collision;
    public Vector2 velocity;
    public Animator animator;
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
    public UnitData data;
    [SerializeField] private MoveState moveState;

    private void Awake() {
        collisionMask = LayerMask.GetMask("UnitCollider");
        interactionMask = LayerMask.GetMask("Interactable");
        moveState = moveState.Initialise(data, animator);
    }

    private void Update()
    {
        // Clear velocity if approximatley zero
        if(data.velocity.sqrMagnitude < 0.01f)
        {
            data.velocity = Vector2.zero;
        }
        
        // Execute movement, recieve next state
        MoveState nextMoveState = moveState.Execute(data, animator);
        if(moveState != nextMoveState)
        {
            // State Updated
            moveState = nextMoveState.Initialise(data, animator);
        }
        
        // Move
        transform.Translate(data.velocity * Time.deltaTime);

        // Apply Drag
        float drag = (data.collision & UnitCollision.Ground) != 0 ? data.stats.groundDrag : data.stats.airDrag;
        data.velocity.x = Mathf.MoveTowards(data.velocity.x, 0.0f, Time.deltaTime * drag);

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
        leftFootGrounded = Physics2D.Raycast(transform.position + new Vector3(-data.stats.feetSeperation * 0.5f, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.down, data.stats.vaultHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(-data.stats.feetSeperation * 0.5f, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.down * data.stats.vaultHeight, Color.red);
        if (leftFootGrounded)
        {
            Debug.DrawRay(transform.position + new Vector3(-data.stats.feetSeperation * 0.5f, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.down * leftFootGrounded.distance, Color.green);
            leftFootDepth = data.stats.vaultHeight - leftFootGrounded.distance;
        }
        // Downwards raycast, right side
        rightFootGrounded = Physics2D.Raycast(transform.position + new Vector3(data.stats.feetSeperation * 0.5f, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.down, data.stats.vaultHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(data.stats.feetSeperation * 0.5f, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.down * data.stats.vaultHeight, Color.red);
        if (rightFootGrounded)
        {
            Debug.DrawRay(transform.position + new Vector3(data.stats.feetSeperation * 0.5f, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.down * rightFootGrounded.distance, Color.green);
            rightFootDepth = data.stats.vaultHeight - rightFootGrounded.distance;
        }
        if (leftFootGrounded || rightFootGrounded)
        {
            // Player is grounded, move them out of the ground
            data.collision |= UnitCollision.Ground;
            float climbDistance = Mathf.Max(leftFootDepth, rightFootDepth);
            data.velocity.y = (climbDistance / Time.unscaledDeltaTime) * data.stats.stepRate;
        }
        else
        {
            // Player is not grounded, apply gravity
            data.collision &= ~UnitCollision.Ground;
            data.velocity.y -= 9.81f * Time.deltaTime;
        }
        #endregion

        #region Wall Collision
        // Left side low
        lowWallLeft = Physics2D.Raycast(transform.position + new Vector3(0, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.left, data.stats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.left * data.stats.size.x * 0.5f, Color.red);
        if (lowWallLeft)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.left * lowWallLeft.distance, Color.green);
            transform.Translate(new Vector3(((data.stats.size.x * 0.5f) - lowWallLeft.distance), 0));
        }
        // Left side high
        highWallLeft = Physics2D.Raycast(transform.position + new Vector3(0, data.stats.size.y * 0.5f), Vector2.left, data.stats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, data.stats.size.y * 0.5f), Vector2.left * data.stats.size.x * 0.5f, Color.red);
        if (highWallLeft)
        {
            Debug.DrawRay(transform.position + new Vector3(0, data.stats.size.y * 0.5f), Vector2.left * highWallLeft.distance, Color.green);
            transform.Translate(new Vector3(((data.stats.size.x * 0.5f) - highWallLeft.distance), 0));
        }
        // Right side low
        lowWallRight = Physics2D.Raycast(transform.position + new Vector3(0, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.right, data.stats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.right * data.stats.size.x * 0.5f, Color.red);
        if (lowWallRight)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(data.stats.size.y * 0.5f) + data.stats.vaultHeight), Vector2.right * lowWallRight.distance, Color.green);
            transform.Translate(new Vector3(-((data.stats.size.x * 0.5f) - lowWallRight.distance), 0));
        }
        // Right side high
        highWallRight = Physics2D.Raycast(transform.position + new Vector3(0, data.stats.size.y * 0.5f), Vector2.right, data.stats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, data.stats.size.y * 0.5f), Vector2.right * data.stats.size.x * 0.5f, Color.red);
        if (highWallRight)
        {
            Debug.DrawRay(transform.position + new Vector3(0, data.stats.size.y * 0.5f), Vector2.right * highWallRight.distance, Color.green);
            transform.Translate(new Vector3(-((data.stats.size.x * 0.5f) - highWallRight.distance), 0));
        }
        #endregion
    }

    public InputData GetInputData()
    {
        return data.input;
    }
}