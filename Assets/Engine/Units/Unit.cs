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
    public bool jumpQueued { get { return Time.unscaledTime - jumpRequestTime < 0.1f; } }
    public float jumpRequestTime = -1.0f;
    public bool running;
    
    public bool crawling { get { return _crawling || crawlLocked; } set { _crawling = value; } }
    private bool _crawling = false;
    public bool crawlLocked = false;
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
    Fall            = 256
}


public class Unit : MonoBehaviour
{

    protected LayerMask collisionMask;
    protected LayerMask interactionMask;

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
    private const float colliderInterpRate = 5.0f;
    private const float groundSpringDistanceBuffer = 0.2f;

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
        ApplyGroundSpring();
        ApplyMovement();
        
        if (colliderInterpValue != (data.isStanding ? 1.0f : 0.0f))
            UpdateCollider();

        // Animate
        animator.SetFloat("VelocityX", data.rb.velocity.x);
        if (data.rb.velocity.x > 0.5f) { data.isFacingRight = true; }
        else if (data.rb.velocity.x < -0.5f) { data.isFacingRight = false; }
        else if (data.input.movement > 0.0f) { data.isFacingRight = true; }
        else if (data.input.movement < 0.0f) { data.isFacingRight = false; }
        animator.SetBool("FacingRight", data.isFacingRight);

        //Debug.DrawRay(transform.position, data.rb.velocity, Color.magenta);
    }
    
    private void ApplyMovement()
    {
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
    }
    
    private void ApplyGroundSpring()
    {
        float springDistance = Mathf.Lerp(data.stats.crawlingSpringDistance, data.stats.standingSpringDistance, colliderInterpValue);
        float springWidth = Mathf.Lerp(data.stats.crawlingSpringWidth, data.stats.standingSpringWidth, colliderInterpValue);
        Vector2 velocity = data.rb.velocity;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(springWidth, springWidth), transform.eulerAngles.z, -transform.up, springDistance - (springWidth * 0.5f) + groundSpringDistanceBuffer, collisionMask);
        if(hit && data.groundSpringActive)
        {
            // Apply spring force
            float springDisplacement = (springDistance - (springWidth * 0.5f)) - hit.distance;
            float springForce = springDisplacement * data.stats.springForce;
            float springDamp = Vector2.Dot(velocity, transform.up) * data.stats.springDamping;
            velocity += (Vector2)transform.up * (springForce - springDamp) * Time.fixedDeltaTime;
            Debug.DrawRay(transform.position - transform.up * springDistance, transform.up * springDisplacement, Color.magenta);
            Debug.DrawRay(hit.point, hit.normal, Color.cyan);

            float groundAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (groundAngle > data.stats.groundedMaxAngle)
            {
                // Ground too steep, dive
                data.isGrounded = false;
                data.input.crawlLocked = true;

                // Apply gravity
                velocity.x += Physics2D.gravity.x * Time.fixedDeltaTime;
                velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;
            }
            else
            {
                data.isGrounded = true;
                data.input.crawlLocked = false;
            }
            Debug.DrawLine(transform.position, hit.point, Color.green);
        }
        else
        {
            // Apply gravity
            velocity.x += Physics2D.gravity.x * Time.fixedDeltaTime;
            velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;
            data.isGrounded = false;
            data.input.crawlLocked = false;
        }
        
        // Rotate Unit
        float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
        if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
        rotationDisplacement -= Vector2.SignedAngle(Vector2.up, hit.normal);
        data.rb.angularVelocity = -rotationDisplacement * (data.isGrounded ? data.stats.groundRotationForce : data.stats.airRotationForce) * Time.fixedDeltaTime;

        // Ceiling check
        if (data.isGrounded)
        {
            float ceilCheckHeight = Mathf.Lerp(data.stats.crawlingCeilingCheckHeight, data.stats.standingCeilingCheckHeight, colliderInterpValue);
            hit = Physics2D.BoxCast(transform.position, new Vector2(springWidth, springWidth), transform.eulerAngles.z, transform.up, ceilCheckHeight - (springWidth * 0.5f), collisionMask);
            if (hit)
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                data.input.crawlLocked = true;
            }
        }

        data.rb.velocity = velocity;
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
    
    public InputData GetInputData()
    {
        return data.input;
    }

}