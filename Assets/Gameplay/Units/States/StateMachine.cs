//#define DEBUG_VAULT
//#define DEBUG_LEDGE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using States;
using System;

[RequireComponent(typeof(Unit))]
public class StateMachine : MonoBehaviour
{
    public UnitState CurrentState => currentState;
    public UnitState PreviousState => previousState;

    public Action<UnitState> OnStateChanged;

    protected Unit unit;
    protected Dictionary<UnitState, BaseState> states = new Dictionary<UnitState, BaseState>();
    protected Dictionary<UnitState, float> statesLastExecutionTime = new Dictionary<UnitState, float>();
    
    private UnitState currentState = UnitState.Idle;
    private UnitState previousState = UnitState.Null;
    private UnitState lastFrameState = UnitState.Null;
    private LayerMask m_EnvironmentMask = 8;

    protected virtual void Start()
    {
        unit = GetComponent<Unit>();

        foreach (UnitState state in Enum.GetValues(typeof(UnitState)))
        {
            statesLastExecutionTime.Add(state, Time.time - 1.0f);
        }
    }

    public void Execute()
    {
        if (currentState != lastFrameState)
        {
            previousState = lastFrameState;
            if (previousState != UnitState.Null)
            {
                statesLastExecutionTime[previousState] = Time.time;
                states[previousState].Deinitialise();
            }
            states[currentState].Initialise();
            OnStateChanged?.Invoke(currentState);
        }
        lastFrameState = currentState;
        currentState = states[currentState].Execute();
    }

    public BaseState GetStateClass(UnitState state)
    {
        return states[state];
    }

    public float GetLastExecutionTime(UnitState state)
    {
        return Time.time - statesLastExecutionTime[state];
    }

    public bool CanCrawl()
    {
        const int iterations = 9;
        const float edgeBuffer = 0.02f;
        float maxOffset = (unit.Collider.Info[BodyState.Crawling].Width * 0.5f) - (unit.Collider.Info[BodyState.Standing].Width * 0.5f) + edgeBuffer;
        for (int i = 0; i <= iterations; i++)
        {
            float iteration = i / (float)iterations;
            float xOffset = Mathf.Lerp(-maxOffset, maxOffset, iteration);
            if (!unit.Collider.Overlap(BodyState.Crawling, new Vector2(xOffset, 0), i == 9)) { return true; }
        }
        return false;
    }

    public bool CanStand()
    {
        if (!unit.GroundSpring.Intersecting) return false;
        float xOffset = (unit.Collider.Info[BodyState.Standing].Width * 0.5f);
        SpringSettings standingSpring = unit.Settings.spring.GetGroundSpring(BodyState.Standing);
        float yOffset = standingSpring.position.magnitude + standingSpring.restDistance - unit.GroundSpring.HitDistance;
        //if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(0, yOffset), true)) { return true; }
        if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(xOffset, yOffset), true)) { return true; }
        //if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(-xOffset, yOffset), true)) { return true; }
        return false;
    }

    public bool TryVaultOn()
    {
        float vaultCheckHeight = unit.Settings.vaultCheckMaxHeight - unit.Settings.vaultCheckMinHeight;
        Vector2 vaultCheckOrigin = new Vector2
        {
            x = unit.FacingRight ? unit.Settings.vaultCheckDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) : -unit.Settings.vaultCheckDistance - (unit.Collider.Info[BodyState.Standing].Width * 0.5f),
            y = -unit.GroundSpring.HitDistance + unit.Settings.vaultCheckMinHeight + vaultCheckHeight
        };
#if DEBUG_VAULT
        Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -transform.up * vaultCheckHeight, Color.blue);
#endif
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)vaultCheckOrigin, -transform.up, vaultCheckHeight, 8);

        if (hit.collider != null)
        {
            if (hit.distance == 0) { return false; }
            const int iterationCount = 10;
            Vector2 maxOrigin = vaultCheckOrigin - new Vector2(unit.FacingRight ? unit.Settings.vaultCheckDistance : -unit.Settings.vaultCheckDistance, 0);
            Debug.DrawRay(transform.position + (Vector3)maxOrigin, -transform.up * vaultCheckHeight, Color.blue);
            for (int i = 1; i < iterationCount; ++i)
            {
                Vector2 iterationOrigin = Vector2.Lerp(vaultCheckOrigin, maxOrigin, (float)i / iterationCount);
                RaycastHit2D iterationHit = Physics2D.Raycast(transform.position + (Vector3)iterationOrigin, -transform.up, vaultCheckHeight, 8);
                if (iterationHit.collider != null) { hit = iterationHit; }
                Debug.DrawRay(transform.position + (Vector3)iterationOrigin, -transform.up * vaultCheckHeight, Color.red);
            }

            DebugExtension.DebugPoint(hit.point, Color.green, 0.1f);
            Vector2 standOffset = new Vector2
            {
                x = 0,
                y = unit.GroundSpring.HitDistance
            };
            if (!unit.Collider.Overlap(BodyState.Standing, (Vector2)transform.InverseTransformPoint(hit.point) + standOffset, true))
            {
                unit.Animator.Play(UnitAnimationState.VaultOn);
                unit.Animator.Translate(hit.point + standOffset, unit.Animator.CurrentStateLength, unit.Collider.OnHit);
                return true;
            }
        }
        return false;
    }

    public bool TryVaultOver()
    {
        float vaultCheckHeight = unit.Settings.vaultCheckMaxHeight - unit.Settings.vaultCheckMinHeight;

        Vector2 vaultCheckOrigin = new Vector2
        {
            x = unit.FacingRight ? unit.Settings.vaultCheckDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) : -unit.Settings.vaultCheckDistance - (unit.Collider.Info[BodyState.Standing].Width * 0.5f),
            y = -unit.GroundSpring.HitDistance + unit.Settings.vaultCheckMinHeight + vaultCheckHeight
        };
#if DEBUG_VAULT
        Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -transform.up * vaultCheckHeight, Color.blue);
#endif
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)vaultCheckOrigin, -transform.up, vaultCheckHeight, 8);

        if (hit.collider != null)
        {
            if (hit.distance == 0) { return false; }
            const int iterationCount = 10;
            Vector2 maxOrigin = vaultCheckOrigin - new Vector2(unit.FacingRight ? unit.Settings.vaultCheckDistance : -unit.Settings.vaultCheckDistance, 0);
            Debug.DrawRay(transform.position + (Vector3)maxOrigin, -transform.up * vaultCheckHeight, Color.blue);
            for (int i = 1; i < iterationCount; ++i)
            {
                Vector2 iterationOrigin = Vector2.Lerp(vaultCheckOrigin, maxOrigin, (float)i / iterationCount);
                RaycastHit2D iterationHit = Physics2D.Raycast(transform.position + (Vector3)iterationOrigin, -transform.up, vaultCheckHeight, 8);
                if (iterationHit.collider != null) { hit = iterationHit; }
                Debug.DrawRay(transform.position + (Vector3)iterationOrigin, -transform.up * vaultCheckHeight, Color.red);
            }

            DebugExtension.DebugPoint(hit.point, Color.green, 0.1f);
            Vector2 standOffset = new Vector2
            {
                x = (unit.FacingRight ? 1 : -1) * unit.Settings.vaultOverDistance,
                y = 0
            };
            Vector2 localHit = unit.transform.InverseTransformPoint(hit.point);
            localHit.y = 0;
            if (!unit.Collider.Overlap(BodyState.Standing, localHit + standOffset , true))
            {
                unit.Animator.Play(UnitAnimationState.VaultOver);
                unit.Animator.Translate((Vector2)transform.position + localHit + standOffset, unit.Animator.CurrentStateLength, unit.Collider.OnHit);
                return true;
            }
        }
        return false;
    }

    public bool TryLedgeGrab()
    {
        const float contactOffset = 0.02f;
        float climbCheckHeight = unit.Settings.climbCheckMaxHeight - unit.Settings.climbCheckMinHeight;

        // Check if a ledge exists by casting a box over the entire region the unit can grab
        Vector2 ledgeCheckOrigin = new Vector2()
        {
            x = (unit.FacingRight ? 1 : -1) * ((unit.Collider.Info[BodyState.Standing].Width * 0.5f) + (unit.Settings.climbRequiredInset.x * 0.5f)),
            y = -unit.GroundSpring.HitDistance + unit.Settings.climbCheckMinHeight + (climbCheckHeight * 0.5f)
        } + (Vector2)unit.transform.position;
#if DEBUG_LEDGE
        DebugExtension.DebugBounds(new Bounds(ledgeCheckOrigin, new Vector2(unit.Settings.climbRequiredInset.x, climbCheckHeight)), Color.blue, tickRate);
#endif
        Vector2 ledgeCheckSize = new Vector2(unit.Settings.climbRequiredInset.x - contactOffset, climbCheckHeight - contactOffset);
        if (!Physics2D.OverlapBox(ledgeCheckOrigin, ledgeCheckSize, transform.eulerAngles.z, m_EnvironmentMask))
        {
            return false;
        }

        // Check for any gaps to grab within the region
        Vector2 minOrigin = new Vector2
        {
            x = (unit.FacingRight ? 1 : -1) * ((unit.Collider.Info[BodyState.Standing].Width * 0.5f) + (unit.Settings.climbRequiredInset.x * 0.5f)),
            y = -unit.GroundSpring.HitDistance + unit.Settings.climbCheckMinHeight + (unit.Settings.climbRequiredInset.y * 0.5f)
        } + (Vector2)unit.transform.position;
        Vector2 maxOrigin = new Vector2
        {
            x = (unit.FacingRight ? 1 : -1) * ((unit.Collider.Info[BodyState.Standing].Width * 0.5f) + (unit.Settings.climbRequiredInset.x * 0.5f)),
            y = -unit.GroundSpring.HitDistance + unit.Settings.climbCheckMinHeight + climbCheckHeight - (unit.Settings.climbRequiredInset.y * 0.5f)
        } + (Vector2)unit.transform.position;

        const int iterations = 9;
        float gapIteration = -1.0f;
        float gapNextIteration = -1.0f;
        Vector2 gapCheckSize = new Vector2(unit.Settings.climbRequiredInset.x - contactOffset, unit.Settings.climbRequiredInset.y - contactOffset);
        for (int i = 1; i < iterations; i++)
        {
            float iteration = (float)i / (iterations - 1);
            Vector2 iterationOrigin = Vector2.Lerp(minOrigin, maxOrigin, iteration);
#if DEBUG_LEDGE
            DebugExtension.DebugBounds(new Bounds(iterationOrigin, unit.Settings.climbRequiredInset), Color.red, TickRate);
#endif
            if (!Physics2D.OverlapBox(iterationOrigin, gapCheckSize, transform.eulerAngles.z))
            {
                gapIteration = iteration;
                gapNextIteration = ((float)i - 1) / (iterations - 1);
#if DEBUG_LEDGE
                DebugExtension.DebugBounds(new Bounds(iterationOrigin, unit.Settings.climbRequiredInset), Color.green, TickRate);
#endif
                break;
            }
        }
        if (gapIteration == -1.0f) { return false; }

        Vector2 gapOrigin = Vector2.Lerp(minOrigin, maxOrigin, gapIteration);
        Vector2 gapNextOrigin = Vector2.Lerp(minOrigin, maxOrigin, gapNextIteration);
        float gapNextDistance = Vector2.Distance(gapOrigin, gapNextOrigin);
        // Boxcast down from the gap to find hit point
        Vector2 verticalHitOrigin = new Vector2
        {
            x = gapOrigin.x + ((unit.FacingRight ? 1 : -1) * unit.Settings.climbRequiredInset.x * 0.5f),
            y = gapOrigin.y - (unit.Settings.climbRequiredInset.y * 0.5f)
        };
        RaycastHit2D verticalHit = Physics2D.Raycast(verticalHitOrigin, -transform.up, gapNextDistance + 0.001f, m_EnvironmentMask);
        if (verticalHit.collider == null) { return false; }
#if DEBUG_LEDGE
        Debug.DrawRay(verticalHitOrigin, -transform.up * verticalHit.distance, Color.green, tickRate);
#endif

        const float horizontalSweepBuffer = 0.01f;
        Vector2 horizontalOrigin = new Vector2
        {
            x = gapOrigin.x + ((unit.FacingRight ? -1 : 1) * (unit.Settings.climbRequiredInset.x * 0.5f) - (horizontalSweepBuffer * 0.5f)),
            y = Mathf.Lerp(gapOrigin.y, gapNextOrigin.y, 0.5f) - (unit.Settings.climbRequiredInset.y * 0.5f)
        };
        Vector2 horizontalSize = new Vector2(horizontalSweepBuffer, gapNextDistance);
        //DebugExtension.DrawBoxCastBox(horizontalOrigin, horizontalSize * 0.5f, transform.rotation, unit.FacingRight ? transform.right : -transform.right, unit.Settings.climbRequiredInset.x, Color.magenta);
        RaycastHit2D horizontalHit = Physics2D.BoxCast(horizontalOrigin, horizontalSize, transform.eulerAngles.z, unit.FacingRight ? transform.right : -transform.right, unit.Settings.climbRequiredInset.x, m_EnvironmentMask);
        
        Vector2 grabPoint = new Vector2
        {
            x = horizontalHit.collider ? horizontalHit.point.x : unit.Settings.climbRequiredInset.x,
            y = verticalHit.point.y
        };
#if DEBUG_LEDGE
        DebugExtension.DebugPoint(grabPoint, Color.magenta, 0.1f, tickRate);
#endif
        Vector2 unitPosition = new Vector2
        {
            x = grabPoint.x - ((unit.FacingRight ? 1 : -1) * unit.Settings.climbGrabOffset.x),
            y = grabPoint.y - unit.Settings.climbGrabOffset.y
        };
        unit.Animator.Translate(unitPosition, 0.0f, unit.Collider.OnHit);

        return true;
    }

    public bool TryClimb()
    {
        Vector2 offset = new Vector2((unit.FacingRight ? 1 : -1) * unit.Settings.climbGrabOffset.x, unit.Settings.climbGrabOffset.y);
        Vector2 target = (Vector2)transform.position + offset + (Vector2)(transform.up * unit.GroundSpring.HitDistance);
        Vector2 colliderOffset = unit.Collider.transform.InverseTransformPoint(target);
        if (!unit.Collider.Overlap(BodyState.Standing, colliderOffset, true))
        {
            unit.Animator.Play(UnitAnimationState.Climb);
            unit.Animator.Translate(target, unit.Animator.CurrentStateLength, unit.Collider.OnHit);
            return true;
        }
        return false;
    }

    public bool TryDrop()
    {
        const float groundHitBuffer = 0.1f;
        Vector2 currentPosition = transform.position;
        Vector2 previousPosition = currentPosition - (unit.Physics.Velocity * Time.fixedDeltaTime);
        RaycastHit2D currentHit = Physics2D.Raycast(currentPosition, -transform.up, unit.GroundSpring.HitDistance + groundHitBuffer, 8);
        RaycastHit2D previousHit = Physics2D.Raycast(previousPosition, -transform.up, unit.GroundSpring.HitDistance + groundHitBuffer, 8);
        if (currentHit.collider == null && previousHit.collider != null)
        {
            Debug.DrawRay(currentPosition, -transform.up * (unit.GroundSpring.HitDistance + groundHitBuffer), Color.red);
            Debug.DrawRay(previousPosition, -transform.up * (unit.GroundSpring.HitDistance + groundHitBuffer), Color.red);

            const int iterationCount = 10;
            RaycastHit2D bestHit = previousHit;
            for (int i = 1; i < iterationCount; ++i)
            {
                Vector2 iterationOrigin = Vector2.Lerp(previousPosition, currentPosition, (float)i / iterationCount);
                RaycastHit2D iterationHit = Physics2D.Raycast(iterationOrigin, -transform.up, unit.GroundSpring.HitDistance + groundHitBuffer, 8);
                if (iterationHit.collider != null)
                {
                    Debug.DrawRay(iterationOrigin, -transform.up * iterationHit.distance, Color.green);
                    bestHit = iterationHit;
                }
                else
                {
                    DebugExtension.DebugPoint(bestHit.point, Color.magenta, 0.1f);
                    Vector2 offset = new Vector2((unit.FacingRight ? 1 : -1) * unit.Settings.climbGrabOffset.x, -unit.Settings.climbGrabOffset.y);
                    Vector2 target = bestHit.point + offset;
                    Vector2 colliderOffset = unit.Collider.transform.InverseTransformPoint(target);
                    if (!unit.Collider.Overlap(BodyState.Standing, colliderOffset, true))
                    {
                        Debug.DrawLine(bestHit.point, target, Color.magenta);
                        unit.Animator.Play(UnitAnimationState.SlideToHang);
                        unit.SetBodyState(BodyState.Standing, unit.Animator.CurrentStateLength);
                        unit.Animator.Translate(target, unit.Animator.CurrentStateLength, unit.Collider.OnHit);
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
