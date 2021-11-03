using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitData
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public UnitState possibleStates;
    [HideInInspector] public UnitState previousState;
    
    public InputData input = new InputData();
    public UnitStats stats;
    public Vector2 target;
    public bool isGrounded = false;
    public bool isStanding = true;
    public bool isFacingRight = true;
    public bool groundSpringActive = true;
    public bool updateFacing = true;
    public float t = 0.0f;

    public void ApplyDrag(float drag)
    {
        // Apply drag
        rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, rb.velocity.magnitude * drag * Time.fixedDeltaTime);
    }
}

[Serializable]
public class InputData
{
    public int movement;
    public bool running;
    
    public bool jumpQueued { get { return Time.unscaledTime - jumpRequestTime < 0.1f; } }
    public float jumpRequestTime = -1.0f;
    
    public bool crawling { get { return Time.unscaledTime - crawlRequestTime < 0.1f || _crawling; } set { _crawling = value; } }
    private bool _crawling = false;
    public float crawlRequestTime = -1.0f;
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
    VaultOnState    = 128,
    Fall            = 256,
    CrawlIdle       = 512,
    LedgeGrab       = 1024,
    WallJump        = 2048,
    Climb           = 4096,
    WallSlide       = 8192,
}

public class Unit : MonoBehaviour
{

    public static LayerMask collisionMask;
    public static LayerMask interactionMask;

    [Header("Components")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator animator;

    [Header("State Data")]
    [SerializeField] private UnitState possibleStates;
    [SerializeField] private UnitState state;
    public UnitData data;
    
    [Header("Collider")]
    [SerializeField] private PolygonCollider2D activeCollider;
    [SerializeField] private Vector2[] standingPoints;
    [SerializeField] private Vector2[] crawlingPoints;
    private float colliderInterpValue = 1.0f;
    private float stateDuration = 0.0f;
    private const float colliderInterpRate = 10.0f;
    private const float groundSpringDistanceBufferStanding = 0.4f;
    private const float groundSpringDistanceBufferCrawling = 0.1f;

    private void Awake() {
        // Init layer masks
        collisionMask = LayerMask.GetMask("UnitCollider");
        interactionMask = LayerMask.GetMask("Interactable");
        
        // Init data
        data.rb = GetComponent<Rigidbody2D>();
        data.animator = animator;
        data.possibleStates = possibleStates;
        
        // Init default state
        UnitStates.Initialise(data, state);
    }

    private void FixedUpdate() 
    {
        if (data.groundSpringActive)
        {
            UpdateGroundSpring();
        }
        else
        {
            data.isGrounded = false;
            // Rotate to default
            float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            rotationDisplacement -= Vector2.SignedAngle(Vector2.up, transform.up);
            data.rb.angularVelocity = -rotationDisplacement * data.stats.airRotationForce * Time.fixedDeltaTime;
        }
        
        // Apply gravity
        data.rb.gravityScale = !data.isGrounded ? 1.0f : 0.0f;
        
        UpdateMovement();
        //UpdateCeiling();
        UpdateAnimation();

        // Dont update collider if we are fully interped
        if (colliderInterpValue != (data.isStanding ? 1.0f : 0.0f))
            UpdateCollider();

        //Debug.DrawRay(transform.position, data.rb.velocity * Time.fixedDeltaTime, Color.grey, 3.0f);
    }
    
    private void UpdateMovement()
    {
        // Log out current state
        stateDuration += Time.fixedDeltaTime;
        Log.UnitState(state, stateDuration);

        // Execute movement, recieve next state
        UnitState nextState = UnitStates.Execute(data, state);
        // State Updated
        if (state != nextState)
        {
            stateDuration = 0.0f;
            // Initialise new state
            data.previousState = state;
            UnitStates.Initialise(data, nextState);
            state = nextState;
        }
    }

    private void UpdateGroundSpring()
    {
        float springDistance = Mathf.Lerp(data.stats.crawlingSpringDistance, data.stats.standingSpringDistance, colliderInterpValue);
        Vector2 springSize = Vector2.Lerp(data.stats.crawlingSpringSize, data.stats.standingSpringSize, colliderInterpValue);
        float groundSpringDistanceBuffer = Mathf.Lerp(groundSpringDistanceBufferCrawling, groundSpringDistanceBufferStanding, colliderInterpValue);
        Vector2 velocity = data.rb.velocity;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, springSize, transform.eulerAngles.z, -transform.up, springDistance - (springSize.y * 0.5f) + groundSpringDistanceBuffer, collisionMask);
        if(hit)
        {
            ExtDebug.DrawBoxCastOnHit(transform.position, new Vector2(springSize.x, springSize.y) * 0.5f, transform.rotation, -transform.up, hit.distance, data.groundSpringActive ? Color.green : Color.gray);
            
            // Apply spring force
            float springDisplacement = (springDistance - (springSize.y * 0.5f)) - hit.distance;
            float springForce = springDisplacement * data.stats.springForce;
            float springDamp = Vector2.Dot(velocity, transform.up) * data.stats.springDamping;

            velocity += (Vector2)transform.up * (springForce - springDamp) * Time.fixedDeltaTime;
            
            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            // Check if we are grounded based on angle of surface
            float groundAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (groundAngle > data.stats.groundedMaxAngle)
            {
                // Surface is not standable, or may have just hit a corner
                // Check for a corner
                Vector2 cornerCheckOrigin = hit.point + (Vector2.up * 0.1f);
                hit = Physics2D.Raycast(cornerCheckOrigin, Vector2.down, 0.15f, collisionMask);
                //Debug.DrawRay(cornerCheckOrigin, Vector2.down * 0.6f, Color.red);
                // Raycast does not hit if we are on a corner
                if (hit)
                {
                    // On Surface, force crawl
                    data.input.crawlRequestTime = Time.unscaledTime;
                }
                data.isGrounded = false;
            }
            else
            {
                // Grounded on standable surface
                //Debug.DrawRay(hit.point, hit.normal, Color.green);
                data.isGrounded = springDisplacement > -0.05f;
            }

            // Rotate Unit
            float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            rotationDisplacement -= Vector2.SignedAngle(Vector2.up, hit.normal);
            float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.isGrounded ? data.stats.groundRotationForce : data.stats.airRotationForce)) - (data.isGrounded ? data.stats.groundRotationDamping : data.stats.airRotationDamping);
            data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
        }
        else
        {
            ExtDebug.DrawBoxCastOnHit(transform.position, springSize * 0.5f, transform.rotation, -transform.up, springDistance - (springSize.y * 0.5f) + groundSpringDistanceBuffer, data.groundSpringActive ? Color.red : Color.gray);
            data.isGrounded = false;
            data.rb.angularVelocity = 0;
        }
        
        data.rb.velocity = velocity;
    }
    
    private void UpdateCeiling()
    {
        // Ceiling check
        if (data.isGrounded)
        {
            float ceilCheckHeight = Mathf.Lerp(data.stats.crawlingCeilingCheckHeight, data.stats.standingCeilingCheckHeight, colliderInterpValue);
            Vector2 springSize = Vector2.Lerp(data.stats.crawlingSpringSize, data.stats.standingSpringSize, colliderInterpValue);
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, springSize, transform.eulerAngles.z, transform.up, ceilCheckHeight - (springSize.y * 0.5f), collisionMask);
            if (hit)
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                data.input.crawlRequestTime = Time.unscaledTime;
            }
        }
    }

    private void UpdateCollider()
    {
        float targetInterpValue = data.isStanding ? 1.0f : 0.0f;
        colliderInterpValue = Mathf.MoveTowards(colliderInterpValue, targetInterpValue, Time.fixedDeltaTime * colliderInterpRate);
        
        Vector2[] points = new Vector2[standingPoints.Length];
        for (int i = 0; i < points.Length; ++i)
        {
            points[i] = Vector2.Lerp(crawlingPoints[i], standingPoints[i], colliderInterpValue);
        }
        activeCollider.points = points;
    }
    
    private void UpdateAnimation()
    {
        // Animate
        animator.SetFloat("VelocityX", data.rb.velocity.x);
        if (data.updateFacing)
        {
            if (data.rb.velocity.x > 0.1f) { data.isFacingRight = true; }
            else if (data.rb.velocity.x < -0.1f) { data.isFacingRight = false; }
            else if (data.input.movement > 0.0f) { data.isFacingRight = true; }
            else if (data.input.movement < 0.0f) { data.isFacingRight = false; }
            animator.SetBool("FacingRight", data.isFacingRight);
        }
    }
    
    public InputData GetInputData()
    {
        return data.input;
    }

}