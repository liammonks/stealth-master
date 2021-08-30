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
    
    protected LayerMask collisionMask;
    protected LayerMask interactionMask;

    [Header("Components")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator animator;

    [Header("Stats")]
    [SerializeField] private UnitStats standingStats;
    [SerializeField] private UnitStats crawlingStats;

    private UnitStats activeStats;
    private UnitState activeState;
    private float statsInterp = 0.0f;
    private Vector2 velocity;
    private bool grounded = false;
    private bool running = false;
    private bool jumping = false; // True from when user hits input to the unit leaving the ground
    private bool sliding = false;
    private bool diving = false;
    private bool vaulting = false;
    private bool canJump = false;
    private bool climbFrame = false;
    private bool updateMovement = true;
    
    // Input
    private bool crawlingInput = false;
    private int moveDirection = 0;

    // Interaction
    protected const float interactionDistance = 1.0f;

    // Constants
    private const float spriteVerticalOffset = 0.0f;
    private const float roofCheckDistance = 0.1f;
    private const float groundFriction = 8.0f;
    private const float slideFriction = 2.0f;
    private const float crawlLockDuration = 0.75f;
    private const float stateTransitionRate = 5.0f;
    private const float maxVaultThickness = 0.51f;
    private const float vaultDuration = 0.4f;
    private const float climbDuration = 0.5f;

    private void Awake() {
        SetState(UnitState.Standing);
        activeStats = standingStats.GetInstance();
        collisionMask = LayerMask.GetMask("UnitCollider");
        interactionMask = LayerMask.GetMask("Interactable");
    }
    
    private void SetState(UnitState state)
    {
        if (activeState == state) { return; }
        activeState = state;
        statsInterp = 0.0f;
    }
    
    protected virtual void Update()
    {
        if(updateMovement)
        {
            MoveUpdate();
        }
        // Interpolate stats
        if (statsInterp < 1.0f)
        {
            statsInterp += Time.deltaTime * stateTransitionRate;
            switch (activeState)
            {
                case UnitState.Standing:
                    activeStats = UnitStats.Interpolate(crawlingStats, standingStats, statsInterp);
                    break;
                case UnitState.Crawling:
                    activeStats = UnitStats.Interpolate(standingStats, crawlingStats, statsInterp);
                    break;
            }
            // Set sprite to ground position
            //spriteTransform.localPosition = new Vector3(0, (-activeStats.size.y * 0.5f) + spriteVerticalOffset, 0);
        }
    }
    
    private void MoveUpdate()
    {
        if (vaulting) { return; }
        // Vars
        RaycastHit2D leftHit, rightHit;
        float leftHitElevation = 0.0f, rightHitElevation = 0.0f;
        bool leftCollision = false, rightCollision = false;

        #region Stand Check
        // Unit no longer wants to crawl, attempt to stand up
        if (activeState == UnitState.Crawling && !crawlingInput && grounded)
        {
            if (!sliding || diving)
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
                    animator.SetBool("Diving", false);
                    diving = false;
                    animator.SetBool("Sliding", false);
                    sliding = false;
                }
            }
        }
        #endregion

        // Climbed an object last frame, cancel out vertical momentum
        if (climbFrame)
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
        leftHit = Physics2D.Raycast(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.down, activeStats.vaultHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.down * activeStats.vaultHeight, Color.red);
        if (leftHit)
        {
            Debug.DrawRay(transform.position + new Vector3(-activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.down * leftHit.distance, Color.green);
            leftHitElevation = activeStats.vaultHeight - leftHit.distance;
        }
        // Downwards raycast, right side
        rightHit = Physics2D.Raycast(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.down, activeStats.vaultHeight, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.down * activeStats.vaultHeight, Color.red);
        if (rightHit)
        {
            Debug.DrawRay(transform.position + new Vector3(activeStats.feetSeperation * 0.5f, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.down * rightHit.distance, Color.green);
            rightHitElevation = activeStats.vaultHeight - rightHit.distance;
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
            float climbDistance = Mathf.Max(leftHitElevation, rightHitElevation);
            if (!jumping)
            {
                // Vault when climbing over an object taller than stepHeight
                if(climbDistance > activeStats.stepHeight)
                {
                    bool vaultLeft = leftHitElevation > rightHitElevation;
                    Vector2 vaultDirection = vaultLeft ? Vector2.left : Vector2.right;
                    float groundClearance = activeStats.vaultHeight - 0.1f;
                    // Get a point half way between the top of the vaultable object and the units feet
                    Vector2 nearCheck = (vaultLeft ? leftHit : rightHit).point + (vaultDirection * maxVaultThickness);
                    Vector2 farCheck = (vaultLeft ? leftHit : rightHit).point + (vaultDirection * (maxVaultThickness + activeStats.feetSeperation));
                    RaycastHit2D nearHit = Physics2D.Raycast(nearCheck, Vector2.down, groundClearance, collisionMask);
                    RaycastHit2D farHit = Physics2D.Raycast(farCheck, Vector2.down, groundClearance, collisionMask);
                    Debug.DrawLine(nearCheck, nearCheck + (Vector2.down * groundClearance), nearHit ? Color.red : Color.green, 2.0f);
                    Debug.DrawLine(farCheck, farCheck + (Vector2.down * groundClearance), farHit ? Color.red : Color.green, 2.0f);
                    if(nearHit || farHit)
                    {
                        // Object is in landing zone
                        // Climb over the object
                        velocity = Vector2.zero;
                        StartCoroutine(Climb((Vector2)transform.position + (Vector2.up * climbDistance) + (vaultDirection * activeStats.feetSeperation)));
                    }
                    else
                    {
                        // Landing zone is clear
                        velocity = Vector2.zero;
                        StartCoroutine(Vault((Vector2)transform.position + (vaultDirection * (maxVaultThickness + (activeStats.feetSeperation )))));
                    }
                }
                else
                {
                    // Step over the object
                    velocity.y = (climbDistance / Time.unscaledDeltaTime) * activeStats.stepRate;
                    climbFrame = true;
                    canJump = true;
                }
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
            if (velocity.y * Time.deltaTime >= leftHit.distance)
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
        leftHit = Physics2D.Raycast(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.left, activeStats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.left * activeStats.size.x * 0.5f, Color.red);
        if (leftHit)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.left * leftHit.distance, Color.green);
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
        rightHit = Physics2D.Raycast(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.right, activeStats.size.x * 0.5f, collisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.right * activeStats.size.x * 0.5f, Color.red);
        if (rightHit)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -(activeStats.size.y * 0.5f) + activeStats.vaultHeight), Vector2.right * rightHit.distance, Color.green);
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
                // Apply drag
                velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * activeStats.groundDrag);
                // Apply movement input
                if (Mathf.Abs(velocity.x) < activeStats.runSpeed)
                {
                    float speed = running ? activeStats.runSpeed : activeStats.walkSpeed;
                    velocity.x += speed * moveDirection * Time.deltaTime * activeStats.groundAuthority;
                    velocity.x = Mathf.Clamp(velocity.x, -speed, speed);
                }
            }
            if (activeState == UnitState.Crawling)
            {
                // Slide when travelling faster than max crawl speed
                if (Mathf.Abs(velocity.x) > activeStats.runSpeed)
                {
                    // Slide drag
                    velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * slideFriction);
                    sliding = true;
                }
                else
                {
                    if(diving)
                    {
                        diving = false;
                        animator.SetBool("Diving", false);
                    }
                    // Apply drag
                    velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * activeStats.groundDrag);
                    // Apply movement input
                    float speed = running ? activeStats.runSpeed : activeStats.walkSpeed;
                    velocity.x += speed * moveDirection * Time.deltaTime * activeStats.groundAuthority;
                    velocity.x = Mathf.Clamp(velocity.x, -speed, speed);
                    sliding = false;
                }
            }
        }
        else
        {
            // Apply drag
            velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * activeStats.airDrag);
            // Allow player to push towards movement speed while in the air
            float speed = running ? activeStats.runSpeed : activeStats.walkSpeed;
            if (Mathf.Abs(velocity.x) < speed)
            {
                velocity.x += speed * moveDirection * Time.deltaTime * activeStats.airAuthority;
                velocity.x = Mathf.Clamp(velocity.x, -speed, speed);
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
        //Debug.DrawLine(transform.position, transform.position + (Vector3)(velocity * Time.deltaTime), Color.Lerp(Color.black, Color.white, velocity.magnitude / (activeStats.runSpeed * 10.0f)), 2.0f);
        transform.Translate(velocity * Time.deltaTime);

        // Animate
        animator.SetFloat("VelocityX", velocity.x);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Sliding", sliding);
    }
    
    protected void SetMoveDirection(int direction)
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
        
        if(crawlingInput && activeState == UnitState.Standing)
        {
            // Crawl if grounded
            if (grounded)
            {
                animator.SetBool("Crawling", true);
                SetState(UnitState.Crawling);
            }
            // If in the air and faster than walk speed, dive
            if (!grounded && velocity.y >= 0 && Mathf.Abs(velocity.x) > activeStats.walkSpeed)
            {
                // Dive
                diving = true;
                animator.SetBool("Diving", true);
                velocity += velocity * activeStats.diveVelocityMultiplier;
                animator.SetBool("Crawling", true);
                SetState(UnitState.Crawling);
            }
        }
        
    }
    
    private IEnumerator Vault(Vector2 target)
    {
        vaulting = true;
        animator.SetBool("Vaulting", true);

        Vector3 startPos = transform.position;
        Vector3 endPos = target;
        endPos.z = startPos.z;
        Debug.DrawLine(startPos, endPos, Color.magenta, 2.0f);

        float t = 0.0f;
        while(t < 1.0f)
        {
            t += Time.deltaTime * (1.0f / climbDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        vaulting = false;
        animator.SetBool("Vaulting", false);
    }

    private IEnumerator Climb(Vector2 target)
    {
        vaulting = true;
        animator.SetBool("Vaulting", true);

        Vector3 startPos = transform.position;
        Vector3 endPos = target;
        endPos.z = startPos.z;
        Debug.DrawLine(startPos, endPos, Color.magenta, 2.0f);

        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (1.0f / vaultDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        vaulting = false;
        animator.SetBool("Vaulting", false);
    }

    protected void SetVisible(bool visible)
    {
        spriteTransform.gameObject.SetActive(visible);
    }

    protected void EnableMovement(bool canMove)
    {
        // Reset velocity
        velocity = Vector2.zero;
        moveDirection = 0;
        updateMovement = canMove;
    }
}
