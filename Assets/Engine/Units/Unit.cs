using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    private enum UnitState
    {
        Standing,
        Crawling
    }

    [Header("Components")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator animator;

    [Header("Stats")]
    [SerializeField] private UnitStats standingStats;
    [SerializeField] private UnitStats crawlingStats;

    [Header("Physics")]
    [SerializeField] protected LayerMask collisionMask;
    [SerializeField] protected LayerMask interactionMask;

    private UnitStats activeStats;
    private UnitState activeState;
    private Vector2 velocity;
    private int moveDirection = 0;
    private bool grounded = false;
    private bool running = false;
    private bool jumping = false;
    private bool sliding = false;
    private bool crawlingInput = false;
    private bool canJump = false;
    private bool leftCollision = false;
    private bool rightCollision = false;
    private bool climbFrame = false;

    // Interaction
    private List<Interactable> interactables = new List<Interactable>();
    protected const float interactionDistance = 1.0f;

    // Constants
    private const float spriteVerticalOffset = 0.1f;
    private const float roofCheckDistance = 0.1f;
    private const float slideFriction = 2.0f;

    private void Awake() {
        SetState(UnitState.Standing);
    }
    
    private void SetState(UnitState state)
    {
        activeState = state;
        // Activate relevant stats for the selected state
        switch (state)
        {
            case UnitState.Standing:
                activeStats = standingStats.GetInstance();
                break;
            case UnitState.Crawling:
                activeStats = crawlingStats.GetInstance();
                break;
        }
        // Initialise stats
        activeStats.climbHeight = activeStats.climbHeight + 0.01f; // Climb over objects of the initially defined climbHeight
        // Set sprite to ground position
        spriteTransform.localPosition = new Vector3(0, (-activeStats.size.y * 0.5f) + spriteVerticalOffset, 0);
    }
    
    protected virtual void Update()
    {
        // Re-usable vars
        RaycastHit2D leftHit, rightHit;
        float leftVelocity = 0.0f, rightVelocity = 0.0f;
        
        #region Stand Check
        // Unit no longer wants to crawl, attempt to stand up
        if(activeState == UnitState.Crawling && !crawlingInput)
        {
            // Check we have room to stand
            leftHit = Physics2D.Raycast(transform.position + new Vector3(-standingStats.feetSeperation * 0.5f, 0.0f), Vector3.up, standingStats.size.y - (crawlingStats.size.y * 0.5f), collisionMask);
            if (leftHit)
            {
                Debug.DrawRay(transform.position + new Vector3(-standingStats.feetSeperation * 0.5f, 0.0f), Vector2.up * leftHit.distance, Color.red);
            }
            rightHit = Physics2D.Raycast(transform.position + new Vector3(standingStats.feetSeperation * 0.5f, 0.0f), Vector3.up, standingStats.size.y - (crawlingStats.size.y * 0.5f), collisionMask);
            if (rightHit)
            {
                Debug.DrawRay(transform.position + new Vector3(standingStats.feetSeperation * 0.5f, 0.0f), Vector2.up * rightHit.distance, Color.red);
            }
            // Unit has room, set to standing
            if (!leftHit && !rightHit)
            {
                SetState(UnitState.Standing);
                animator.SetBool("Crawling", false);
            }
        }
        #endregion
        
        // Climbed an object last frame, cancel out vertical momentum
        if(climbFrame)
        {
            velocity.y = 0;
            climbFrame = false;
        }
        // Wall collision last frame, cancel out horizontal momentum
        if (leftCollision || rightCollision)
        {
            velocity.x = 0;
            leftCollision = false;
            rightCollision = false;
        }

        #region Ground Check
        // Downwards raycast, left side
        leftHit = Physics2D.Raycast(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.down, activeStats.climbHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.down * activeStats.climbHeight, Color.red);
        if (leftHit)
        {
            Debug.DrawRay(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.down * leftHit.distance, Color.green);
            float incline = activeStats.climbHeight - leftHit.distance;
            leftVelocity = incline / Time.deltaTime;
        }
        // Downwards raycast, right side
        rightHit = Physics2D.Raycast(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.down, activeStats.climbHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.down * activeStats.climbHeight, Color.red);
        if (rightHit)
        {
            Debug.DrawRay(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.down * rightHit.distance, Color.green);
            float incline = activeStats.climbHeight - rightHit.distance;
            rightVelocity = incline / Time.deltaTime;
        }
        if (!leftHit && !rightHit)
        {
            // Neither ground ray hit
            grounded = false;
            // If the player was jumping, they have now left the ground so we disable jumping
            jumping = false;
        }
        else
        {
            // Contact with ground
            grounded = true;
            // Determine how much we need to push the player up
            float climbVelocity = Mathf.Max(leftVelocity, rightVelocity) * activeStats.climbRate;
            if (!jumping)
            {
                velocity.y = climbVelocity;
                climbFrame = true;
                canJump = true;
            }
            else
            {
                // Jump
                velocity.y = activeStats.jumpForce;
            }
        }
        #endregion

        #region Roof Check
        // Upwards raycast, left side
        leftHit = Physics2D.Raycast(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, activeStats.size.y * 0.5f), Vector2.up, roofCheckDistance, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, activeStats.size.y * 0.5f), Vector2.up * roofCheckDistance, Color.red);
        if (leftHit)
        {
            Debug.DrawRay(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, activeStats.size.y * 0.5f), Vector2.up * leftHit.distance, Color.green);
            if(velocity.y * Time.deltaTime >= leftHit.distance)
            {
                velocity.y = leftHit.distance * Time.deltaTime;
            }
        }
        // Upwards raycast, right side
        rightHit = Physics2D.Raycast(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, activeStats.size.y * 0.5f), Vector2.up, roofCheckDistance, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, activeStats.size.y * 0.5f), Vector2.up * roofCheckDistance, Color.red);
        if (rightHit)
        {
            Debug.DrawRay(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, activeStats.size.y * 0.5f), Vector2.up * rightHit.distance, Color.green);
            if (velocity.y * Time.deltaTime >= rightHit.distance)
            {
                velocity.y = rightHit.distance * Time.deltaTime;
            }
        }
        #endregion

        #region Wall Collision
        // Left side low
        leftHit = Physics2D.Raycast(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.left, activeStats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.left * activeStats.size.x * 0.5f, Color.red);
        if (leftHit)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.left * leftHit.distance, Color.green);
            // Move out of collision
            transform.Translate(new Vector3(((activeStats.size.x * 0.5f) - leftHit.distance), 0));
            leftCollision = true;
        }
        // Left side high
        leftHit = Physics2D.Raycast(transform.position + new Vector3(0, activeStats.size.y * 0.5f), Vector2.left, activeStats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, activeStats.size.y * 0.5f), Vector2.left * activeStats.size.x * 0.5f, Color.red);
        if (leftHit)
        {
            Debug.DrawRay(transform.position + new Vector3(0, activeStats.size.y * 0.5f), Vector2.left * leftHit.distance, Color.green);
            // Move out of collision
            transform.Translate(new Vector3(((activeStats.size.x * 0.5f) - leftHit.distance), 0));
            leftCollision = true;
        }
        
        // Right side low
        rightHit = Physics2D.Raycast(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.right, activeStats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.right * activeStats.size.x * 0.5f, Color.red);
        if (rightHit)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.climbHeight), Vector2.right * rightHit.distance, Color.green);
            // Move out of collision
            transform.Translate(new Vector3(-((activeStats.size.x * 0.5f) - rightHit.distance), 0));
            rightCollision = true;
        }
        // Right side high
        rightHit = Physics2D.Raycast(transform.position + new Vector3(0, activeStats.size.y * 0.5f), Vector2.right, activeStats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, activeStats.size.y * 0.5f), Vector2.right * activeStats.size.x * 0.5f, Color.red);
        if (rightHit)
        {
            Debug.DrawRay(transform.position + new Vector3(0, activeStats.size.y * 0.5f), Vector2.right * rightHit.distance, Color.green);
            // Move out of collision
            transform.Translate(new Vector3(-((activeStats.size.x * 0.5f) - rightHit.distance), 0));
            rightCollision = true;
        }
        #endregion

        #region Input Movement
        if (grounded)
        {
            // Ground movement
            if (activeState == UnitState.Standing)
            {
                // Normal movement input
                velocity.x = (running ? activeStats.runSpeed : activeStats.walkSpeed) * moveDirection;
                sliding = false;
            }
            if (activeState == UnitState.Crawling)
            {
                // Slide when travelling faster than max crawl speed
                if(Mathf.Abs(velocity.x) > activeStats.runSpeed)
                {
                    // Sliding
                    velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * slideFriction);
                    sliding = true;
                }
                else
                {
                    // Crawling
                    velocity.x = (running ? activeStats.runSpeed : activeStats.walkSpeed) * moveDirection;
                    sliding = false;
                }
            }
        }
        else
        {
            // Air movement
            if (Mathf.Abs(velocity.x) < activeStats.walkSpeed)
            {
                // Allow player to push towards movement speed while in the air
                velocity.x += (running ? activeStats.runSpeed : activeStats.walkSpeed) * moveDirection * Time.deltaTime * activeStats.airAuthority;
            }
            else
            {
                // Apply drag
                velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * activeStats.airDrag);
            }
            // Apply gravity
            velocity.y -= 9.81f * Time.deltaTime;
        }
        #endregion
        
        #region Apply Collisions
        // Collisions may exceed terminal velocity
        if (!leftCollision && !rightCollision)
        {
            // Terminal velocity
            velocity = Vector2.ClampMagnitude(velocity, activeStats.terminalVeloicty);
        }
        // Collisions clamp velocity
        velocity.x = leftCollision ? Mathf.Max(0, velocity.x) : velocity.x;
        velocity.x = rightCollision ? Mathf.Min(0, velocity.x) : velocity.x;
        #endregion
        
        // Move
        transform.Translate(velocity * Time.deltaTime);
        
        // Animate
        animator.SetFloat("VelocityX", velocity.x);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Sliding", sliding);
    }
    
    protected void SetMovement(int direction)
    {
        moveDirection = direction;
    }
    
    protected void SetRunning(bool isRunning)
    {
        running = isRunning;
    }

    protected void Jump()
    {
        if (!canJump || activeState == UnitState.Crawling) { return; }
        canJump = false;
        grounded = false;
        jumping = true;
        animator.SetTrigger("Jump");
    }

    protected void Crawl(bool isCrawling)
    {
        crawlingInput = isCrawling;
        
        if(crawlingInput)
        {
            if (grounded)
            {
                // Crawl
                SetState(UnitState.Crawling);
                animator.SetBool("Crawling", true);
            }
            else
            {
                // Dive
            }
        }
        
    }
    
}
