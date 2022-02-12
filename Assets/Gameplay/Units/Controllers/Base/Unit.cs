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
    public float lastGroundedTime = 0.0f;
    public bool canJump => (Time.unscaledTime - lastGroundedTime) <= 0.2f;
    public bool isStanding = true;
    public bool isFacingRight = true;
    public bool isSlipping = false;
    public bool groundSpringActive = true;
    public float t = 0.0f;
    public float stateDuration = 0.0f;
    public List<uint> hitIDs = new List<uint>();
    public Rigidbody2D attatchedRB;

    public delegate void OnLockGadget();
    public event OnLockGadget lockGadget;
    public void LockGadget()
    {
        lockGadget?.Invoke();
    }
    
    public delegate void OnUnlockGadget();
    public event OnUnlockGadget unlockGadget;
    public void UnlockGadget()
    {
        unlockGadget?.Invoke();
    }

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

    public float movement;
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

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Unit : MonoBehaviour
{
    public static LayerMask CollisionMask => m_CollisionMask;
    private static LayerMask m_CollisionMask;

    public static LayerMask InteractionMask => m_InteractionMask;
    private static LayerMask m_InteractionMask;

    public delegate void OnDamageTaken();
    public event OnDamageTaken onDamageTaken;

    public delegate void OnAimOffsetUpdated();
    public event OnAimOffsetUpdated onAimOffsetUpdated;

    public uint ID;

    [Header("State Data")]
    public UnitData data;
    [HideInInspector] public StateMachine stateMachine;

    [Header("Collider")]
    [SerializeField] private PolygonCollider2D activeCollider;
    [SerializeField] private Vector2[] standingPoints;
    [SerializeField] private Vector2[] crawlingPoints;
    private float colliderInterpValue = 1.0f;
    private const float colliderInterpRate = 10.0f;
    private const float groundSpringDistanceBufferStanding = 0.4f;
    private const float groundSpringDistanceBufferCrawling = 0.1f;
    private const float impactRateThreshold = 10.0f;

    [Header("Interaction")]
    [SerializeField] private List<Interactable> interactables = new List<Interactable>();
    private bool lockedRB = false;

    // Health
    protected HealthBar healthBar;
    protected float health;

    // Gadgets
    public Vector2 AimOffset => m_AimOffset;
    private Vector2 m_AimOffset;    
    private Gadgets.BaseGadget equippedGadget;

    // Networking
    [HideInInspector] public NetworkPlayer networkPlayer;

    protected virtual void Awake()
    {
        ID = UnitHelper.GetUnitID();
        
        // Init layer masks
        m_CollisionMask = LayerMask.GetMask("UnitCollider");
        m_InteractionMask = LayerMask.GetMask("Interactable");
        
        // Init data
        data.rb = GetComponent<Rigidbody2D>();
        data.animator = GetComponentInChildren<UnitAnimator>();
        stateMachine = GetComponent<StateMachine>();

        // Init gadgets
        EquipGadget(GlobalData.DefaultGadget);

        health = data.stats.maxHealth;

        onAimOffsetUpdated += UpdateFacing;
        data.animator.onFacingUpdated += UpdateFacing;
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
            SetGrounded(false);
            // Rotate to default
            float rotationDisplacement = transform.eulerAngles.z; // 0 to 360
            if (rotationDisplacement >= 180) { rotationDisplacement = rotationDisplacement - 360; } // -180 to 180
            float rotationForce = (-(rotationDisplacement / Time.fixedDeltaTime) * (data.stats.airRotationForce)) - (data.stats.airRotationDamping);
            data.rb.angularVelocity = rotationForce * Time.fixedDeltaTime;
        }
        
        // Apply gravity
        data.rb.gravityScale = !data.isGrounded ? 1.0f : 0.0f;

        // Dont update collider if we are fully interped
        if (colliderInterpValue != (data.isStanding ? 1.0f : 0.0f))
            UpdateCollider();

        Debug.DrawRay(transform.position, data.rb.velocity * Time.fixedDeltaTime, Color.grey, 3.0f);
    }
    
    private void Update()
    {
        if (data.attatchedRB != null)
        {
            //Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);
            //Log.Text("GroundRB" + ID, data.attatchedRB.transform.name, pos, Color.green, 0);
            data.rb.position += (data.attatchedRB.velocity * Time.deltaTime);
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
                SetGrounded(false);
            }
            else
            {
                SetGrounded(springDisplacement > -0.05f);
                data.attatchedRB = hit.rigidbody;
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
            SetGrounded(false);

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
    
    private void SetGrounded(bool grounded)
    {
        if (data.isGrounded && !grounded) data.lastGroundedTime = Time.unscaledTime;
        data.isGrounded = grounded;
    }

    #endregion

    #region Combat

    public void TakeDamage(float damage)
    {
        health = Mathf.Max(0, health - damage);
        stateMachine.SetState(UnitState.HitImpact);
        if (healthBar != null)
        {
            healthBar.UpdateHealth(health, data.stats.maxHealth);
        }
        if (health == 0)
        {
            Die();
        }
        if (onDamageTaken != null)
        {
            onDamageTaken.Invoke();
        }
    }

    public void TakeDamage(Vector2 impactVelocity)
    {
        data.rb.velocity += impactVelocity;
        if (impactVelocity.magnitude < impactRateThreshold) { return; }
        stateMachine.SetState(UnitState.Launched);
        float impactDamage = impactVelocity.magnitude * data.stats.collisionDamageMultiplier;
        health = Mathf.Max(0, health - impactDamage);
        if (healthBar != null)
        {
            healthBar.UpdateHealth(health, data.stats.maxHealth);
        }
        if (health == 0)
        {
            UnitHelper.Instance.SpawnGibs(transform.position, impactVelocity.magnitude);
            Die();
        }
        if (onDamageTaken != null)
        {
            onDamageTaken.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Vector2 impactVelocity = other.relativeVelocity * (other.rigidbody ? other.rigidbody.mass : 1.0f);
        TakeDamage(impactVelocity * 0.1f);
    }

    public virtual void Die()
    {
        GadgetPrimary(false);
        GadgetSecondary(false);
    }

    #endregion

    #region Gadgets

    private Coroutine enableGadgetPrimary, enableGadgetSecondary;

    public bool EquipGadget(Gadgets.BaseGadget toEquip)
    {
        if (equippedGadget != null)
        {
            if (equippedGadget.PrimaryActive || equippedGadget.SecondaryActive || stateMachine.State == UnitState.Null)
            {
                return false;
            }
            DestroyImmediate(equippedGadget.gameObject);
            data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, Quaternion.identity);
            data.animator.RotateLayer(UnitAnimatorLayer.BackArm, Quaternion.identity);
        }
        equippedGadget = Instantiate(toEquip, transform);
        equippedGadget.Equip(this);
        return true;
    }
    
    public Gadgets.BaseGadget GetEquippedGadget()
    {
        return equippedGadget;
    }

    public void GadgetPrimary(bool active)
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

    public void GadgetSecondary(bool active)
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
    
    public void SetAimOffset(Vector2 offset)
    {
        m_AimOffset = offset;
        onAimOffsetUpdated?.Invoke();
    }
    
    public bool AimingBehind()
    {
        return (data.isFacingRight && AimOffset.x < 0) || (!data.isFacingRight && AimOffset.x > 0);
    }

    public void UpdateFacing()
    {
        data.animator.SetLayer(UnitAnimatorLayer.Body, AimingBehind() ? data.animator.reversedBody : data.animator.defaultBody);
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
        return stateMachine.State;
    }

    public void SetState(UnitState toSet)
    {
        stateMachine.SetState(toSet);
    }
    
    #endregion
}