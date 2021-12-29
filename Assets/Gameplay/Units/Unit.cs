using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitData
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public UnitState previousState;
    [HideInInspector] public LayerMask hitMask;
    [HideInInspector] public UnitAnimator animator;

    public InputData input = new InputData();
    public UnitStats stats;
    public Vector2 target;
    public bool isGrounded = false;
    public bool isStanding = true;
    public bool isFacingRight = true;
    public bool isSlipping = false;
    public bool groundSpringActive = true;
    public float t = 0.0f;
    public float stateDuration = 0.0f;
    public List<uint> hitIDs = new List<uint>();

    public void ApplyDrag(float drag)
    {
        // Apply drag
        rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, rb.velocity.magnitude * drag * Time.fixedDeltaTime);
    }
}

[Serializable]
public class InputData
{
    public static readonly float tolerance = 0.1f;

    public int movement;
    public bool running;

    public bool jumpQueued { get { return Time.unscaledTime - jumpRequestTime < tolerance; } }
    public float jumpRequestTime = -1.0f;

    public bool crawling { get { return Time.unscaledTime - crawlRequestTime < tolerance || _crawling; } set { _crawling = value; } }
    private bool _crawling = false;
    public float crawlRequestTime = -1.0f;

    public bool meleeQueued { get { return Time.unscaledTime - meleeRequestTime < tolerance; } }
    public float meleeRequestTime = -1.0f;

    public void Reset()
    {
        movement = 0;
        running = false;
        jumpRequestTime = -1;
        crawling = false;
        crawlRequestTime = -1;
    }
}

public enum UnitState
{
    Null,
    Idle,
    Run,
    Crawl,
    Slide,
    Dive,
    Jump,
    VaultOverState,
    VaultOnState,
    Fall,
    CrawlIdle,
    LedgeGrab,
    WallJump,
    Climb,
    WallSlide,
    Melee,
    JumpMelee,
    GrappleHookSwing,
    HitImpact,
    Launched
}

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Unit : MonoBehaviour
{
    public static LayerMask CollisionMask => m_CollisionMask;
    private static LayerMask m_CollisionMask;
    
    public static LayerMask InteractionMask => m_InteractionMask;
    private static LayerMask m_InteractionMask;

    public delegate void OnDamageTaken();
    public event OnDamageTaken onDamageTaken;

    public uint ID;

    [Header("State Data")]
    [SerializeField] protected UnitState state;
    public UnitData data;

    [Header("Collider")]
    [SerializeField] private PolygonCollider2D activeCollider;
    [SerializeField] private Vector2[] standingPoints;
    [SerializeField] private Vector2[] crawlingPoints;
    private float colliderInterpValue = 1.0f;
    private const float colliderInterpRate = 10.0f;
    private const float groundSpringDistanceBufferStanding = 0.4f;
    private const float groundSpringDistanceBufferCrawling = 0.1f;

    [Header("Interaction")]
    [SerializeField] private List<Interactable> interactables = new List<Interactable>();
    private bool lockedRB = false;

    protected HealthBar healthBar;
    protected float health;
    private Gadgets.BaseGadget equippedGadget;
    private const float impactRateThreshold = 10.0f;

    protected virtual void Start()
    {
        ID = UnitHelper.GetUnitID();
        
        // Init layer masks
        m_CollisionMask = LayerMask.GetMask("UnitCollider");
        m_InteractionMask = LayerMask.GetMask("Interactable");
        
        // Init data
        data.rb = GetComponent<Rigidbody2D>();
        data.animator = GetComponentInChildren<UnitAnimator>();

        // Init default state
        UnitStates.Initialise(data, state);

        // Init gadgets
        EquipGadget(GlobalData.DefaultGadget);

        health = data.stats.maxHealth;
    }
    
    #region Controller

    protected virtual void FixedUpdate()
    {
        if (lockedRB) { return; }
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
            float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.stats.airRotationForce)) - (data.stats.airRotationDamping);
            data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
        }

        // Apply gravity
        data.rb.gravityScale = !data.isGrounded ? 1.0f : 0.0f;

        UpdateState();

        // Dont update collider if we are fully interped
        if (colliderInterpValue != (data.isStanding ? 1.0f : 0.0f))
            UpdateCollider();

        //Debug.DrawRay(transform.position, data.rb.velocity * Time.fixedDeltaTime, Color.grey, 3.0f);
    }

    private void UpdateState()
    {
        // Log out current state
        data.stateDuration += Time.fixedDeltaTime;

        // Execute movement, recieve next state
        UnitState nextState = UnitStates.Execute(data, state);

        // State Updated
        if (state != nextState)
        {
            data.stateDuration = 0.0f;
            // Initialise new state
            data.previousState = state;
            UnitStates.Initialise(data, nextState);
            state = nextState;
        }
    }

    private void UpdateGroundSpring()
    {
        float springDistance = Mathf.Lerp(data.stats.crawlingHalfHeight, data.stats.standingHalfHeight, colliderInterpValue);
        Vector2 springSize = Vector2.Lerp(data.stats.crawlingSpringSize, data.stats.standingSpringSize, colliderInterpValue);
        float groundSpringDistanceBuffer = Mathf.Lerp(groundSpringDistanceBufferCrawling, groundSpringDistanceBufferStanding, colliderInterpValue);
        Vector2 velocity = data.rb.velocity;
        data.isSlipping = false;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, springSize, transform.eulerAngles.z, -transform.up, springDistance - (springSize.y * 0.5f) + groundSpringDistanceBuffer, CollisionMask);
        if (hit)
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
                hit = Physics2D.Raycast(cornerCheckOrigin, Vector2.down, 0.15f, CollisionMask);
                //Debug.DrawRay(cornerCheckOrigin, Vector2.down * 0.6f, Color.red);
                // Raycast does not hit if we are on a corner
                if (hit)
                {
                    // On Surface, force crawl
                    data.input.crawlRequestTime = Time.unscaledTime;
                    data.isSlipping = true;
                }
                data.isGrounded = false;
            }
            else
            {
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

            // Rotate to default
            float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.stats.airRotationForce)) - (data.stats.airRotationDamping);
            data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
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

    #endregion

    #region Combat

    public void TakeDamage(float damage)
    {
        health = Mathf.Max(0, health - damage);
        state = UnitStates.Initialise(data, UnitState.HitImpact);
        if (healthBar != null)
        {
            healthBar.UpdateHealth(health, data.stats.maxHealth);
        }
        if (health == 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage, Vector2 velocity)
    {
        data.rb.velocity += velocity;
        state = UnitStates.Initialise(data, UnitState.Launched);
        health = Mathf.Max(0, health - damage);
        if (healthBar != null)
        {
            healthBar.UpdateHealth(health, data.stats.maxHealth);
        }
        if (health == 0)
        {
            Die();
        }
    }

    public abstract void Die();

    private void OnCollisionEnter2D(Collision2D other)
    {
        AnimatedRigidbody animatedRigidbody = other.gameObject.GetComponent<AnimatedRigidbody>();
        float impactRate = animatedRigidbody ? (data.rb.velocity - animatedRigidbody.velocity).magnitude : other.relativeVelocity.magnitude;

        if (impactRate > impactRateThreshold) {
            float impactDamage = (impactRate * data.stats.collisionDamageMultiplier) * (animatedRigidbody ? animatedRigidbody.mass : other.rigidbody.mass);
            TakeDamage(impactDamage, other.relativeVelocity.normalized * impactDamage);
            onDamageTaken.Invoke();
        }
    }

    #endregion

    #region Gadgets

    private Coroutine enableGadgetPrimary, enableGadgetSecondary;

    public bool EquipGadget(Gadgets.BaseGadget toEquip)
    {
        if (equippedGadget != null)
        {
            if (equippedGadget.PrimaryActive || equippedGadget.SecondaryActive || state == UnitState.Null)
            {
                return false;
            }
            data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, Quaternion.identity);
            data.animator.RotateLayer(UnitAnimatorLayer.BackArm, Quaternion.identity);
            Destroy(equippedGadget.gameObject);
        }
        equippedGadget = Instantiate(toEquip, transform);
        equippedGadget.Equip(this);
        return true;
    }

    protected void GadgetPrimary(bool active)
    {
        if (active)
        {
            enableGadgetPrimary = StartCoroutine(EnableGadgetPrimary());
        }
        else
        {
            if (enableGadgetPrimary != null)
            {
                StopCoroutine(enableGadgetPrimary);
            }
            equippedGadget.DisablePrimary();
        }
    }

    private IEnumerator EnableGadgetPrimary()
    {
        float t = InputData.tolerance;
        while (t > 0.0f)
        {
            if (equippedGadget == null) { break; }
            equippedGadget.EnablePrimary();
            t -= Time.deltaTime;
            yield return null;
        }
    }

    protected void GadgetSecondary(bool active)
    {
        if (active)
        {
            enableGadgetSecondary = StartCoroutine(EnableGadgetSecondary());
        }
        else
        {
            if (enableGadgetSecondary != null)
            {
                StopCoroutine(enableGadgetSecondary);
            }
            equippedGadget.DisableSecondary();
        }
    }

    private IEnumerator EnableGadgetSecondary()
    {
        float t = InputData.tolerance;
        while (t > 0.0f)
        {
            if (equippedGadget == null) { break; }
            equippedGadget.EnableSecondary();
            t -= Time.deltaTime;
            yield return null;
        }
    }
    
    #endregion

    #region Interaction

    public void AddInteractable(Interactable interactable)
    {
        interactables.Add(interactable);
    }

    public void RemoveInteractable(Interactable interactable)
    {
        interactables.Remove(interactable);
    }

    public bool Interact()
    {
        // Interact with the nearest available interactable
        float nearestInteractableDistance = Mathf.Infinity;
        Interactable nearestInteractable = null;
        foreach (Interactable interactable in interactables)
        {
            float dist = (interactable.transform.position - transform.position).sqrMagnitude;
            if (dist < nearestInteractableDistance)
            {
                nearestInteractable = interactable;
                nearestInteractableDistance = dist;
            }
        }
        if (nearestInteractable != null)
        {
            nearestInteractable.Interact(this);
            return true;
        }
        return false;
    }

    #endregion

    #region Helpers

    public InputData GetInputData()
    {
        return data.input;
    }

    public void LockRB(bool locked)
    {
        lockedRB = locked;
        data.rb.velocity = Vector2.zero;
        data.rb.angularVelocity = 0;
        data.rb.constraints = locked ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
    }

    public UnitState GetState()
    {
        return state;
    }

    public void SetState(UnitState toSet)
    {
        data.stateDuration = 0.0f;
        data.previousState = state;
        state = UnitStates.Initialise(data, toSet);
    }
    
    #endregion
}