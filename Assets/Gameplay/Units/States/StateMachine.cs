using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using States;

[RequireComponent(typeof(Unit))]
public class StateMachine : MonoBehaviour
{
    public float TickRate => tickRate;
    public UnitState CurrentState => currentState;
    public UnitState PreviousState => previousState;

    protected Unit unit;
    protected Dictionary<UnitState, BaseState> states = new Dictionary<UnitState, BaseState>();
    

    private float tickRate;
    private float tickTimer;
    private UnitState currentState = UnitState.Idle;
    private UnitState previousState = UnitState.Null;
    private UnitState lastFrameState = UnitState.Null;

    protected virtual void Start()
    {
        unit = GetComponent<Unit>();
        tickRate = unit.Input.PlayerControlled ? 0.0f : 1.0f / 24.0f;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            tickTimer = 0;
            Tick();
        }
    }

    private void Tick()
    {
        if (currentState != lastFrameState)
        {
            previousState = lastFrameState;
            if (previousState != UnitState.Null) { states[previousState].Deinitialise(); }
            states[currentState].Initialise();
        }
        lastFrameState = currentState;
        currentState = states[currentState].Execute();
    }

    public bool AgainstWall()
    {
        const float edgeBuffer = 0.02f;
        return unit.Collider.Overlap(BodyState.Standing, new Vector2(unit.FacingRight ? edgeBuffer : -edgeBuffer, 0), new Vector2(1.0f, 0.25f), true);
    }

    public bool CanCrawl()
    {
        const float edgeBuffer = 0.02f;
        float xOffset = (unit.Collider.Info[BodyState.Crawling].Width * 0.5f) - (unit.Collider.Info[BodyState.Standing].Width * 0.5f) + edgeBuffer;
        if (!unit.Collider.Overlap(BodyState.Crawling, new Vector2(0, 0), true)) { return true; }
        if (!unit.Collider.Overlap(BodyState.Crawling, new Vector2(xOffset, 0), true)) { return true; }
        if (!unit.Collider.Overlap(BodyState.Crawling, new Vector2(-xOffset, 0), true)) { return true; }
        return false;
    }

    public bool CanStand()
    {
        const float edgeBuffer = 0.02f;
        if (!unit.GroundSpring.Grounded) return false;
        float xOffset = (unit.Collider.Info[BodyState.Standing].Width * 0.5f);
        float yOffset = (unit.Collider.Info[BodyState.Standing].Height * 0.5f) - unit.GroundSpring.GroundDistance + edgeBuffer;
        //if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(0, yOffset), true)) { return true; }
        if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(xOffset, yOffset), true)) { return true; }
        //if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(-xOffset, yOffset), true)) { return true; }
        return false;
    }

    public bool TryVaultOn()
    {
        float vaultCheckHeight = unit.Settings.vaultMaxHeight - unit.Settings.vaultMinHeight;

        Vector2 vaultCheckOrigin = new Vector2
        {
            x = unit.FacingRight ? unit.Settings.vaultGrabDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) : -unit.Settings.vaultGrabDistance - (unit.Collider.Info[BodyState.Standing].Width * 0.5f),
            y = -unit.GroundSpring.GroundDistance + unit.Settings.vaultMinHeight + vaultCheckHeight
        };
        Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -transform.up * vaultCheckHeight, Color.blue);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)vaultCheckOrigin, -transform.up, vaultCheckHeight, 8);

        if (hit.collider != null)
        {
            if (hit.distance == 0) { return false; }
            const int iterationCount = 10;
            Vector2 maxOrigin = vaultCheckOrigin - new Vector2(unit.FacingRight ? unit.Settings.vaultGrabDistance : -unit.Settings.vaultGrabDistance, 0);
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
                y = unit.GroundSpring.GroundDistance
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
        float vaultCheckHeight = unit.Settings.vaultMaxHeight - unit.Settings.vaultMinHeight;

        Vector2 vaultCheckOrigin = new Vector2
        {
            x = unit.FacingRight ? unit.Settings.vaultGrabDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) : -unit.Settings.vaultGrabDistance - (unit.Collider.Info[BodyState.Standing].Width * 0.5f),
            y = -unit.GroundSpring.GroundDistance + unit.Settings.vaultMinHeight + vaultCheckHeight
        };
        Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -transform.up * vaultCheckHeight, Color.blue);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)vaultCheckOrigin, -transform.up, vaultCheckHeight, 8);

        if (hit.collider != null)
        {
            if (hit.distance == 0) { return false; }
            const int iterationCount = 10;
            Vector2 maxOrigin = vaultCheckOrigin - new Vector2(unit.FacingRight ? unit.Settings.vaultGrabDistance : -unit.Settings.vaultGrabDistance, 0);
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

    public bool CanClimb()
    {
        float climbCheckHeight = unit.Settings.climbMaxHeight - unit.Settings.climbMinHeight;
        Vector2 climbCheckOrigin = new Vector2
        {
            x = unit.FacingRight ? unit.Settings.climbGrabDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) : -unit.Settings.climbGrabDistance - (unit.Collider.Info[BodyState.Standing].Width * 0.5f),
            y = -unit.GroundSpring.GroundDistance + unit.Settings.climbMinHeight + climbCheckHeight
        };
        Debug.DrawRay((Vector2)unit.transform.position + climbCheckOrigin, Vector2.down * climbCheckHeight, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)climbCheckOrigin, -transform.up, climbCheckHeight, 8);

        if (hit.collider != null)
        {
            if (hit.distance == 0) { return false; }
            const int iterationCount = 10;
            Vector2 maxOrigin = climbCheckOrigin - new Vector2(unit.FacingRight ? unit.Settings.climbGrabDistance : -unit.Settings.climbGrabDistance, 0);
            Debug.DrawRay(transform.position + (Vector3)maxOrigin, -transform.up * climbCheckHeight, Color.red);
            for (int i = 1; i < iterationCount; ++i)
            {
                Vector2 iterationOrigin = Vector2.Lerp(climbCheckOrigin, maxOrigin, (float)i / iterationCount);
                RaycastHit2D iterationHit = Physics2D.Raycast(transform.position + (Vector3)iterationOrigin, -transform.up, climbCheckHeight, 8);
                if (iterationHit.collider != null) { hit = iterationHit; }
                Debug.DrawRay(transform.position + (Vector3)iterationOrigin, -transform.up * climbCheckHeight, Color.blue);
            }

            DebugExtension.DebugPoint(hit.point, Color.green, 0.1f);
            Vector2 standOffset = new Vector2
            {
                x = 0,
                y = unit.GroundSpring.GroundDistance
            };
            Vector2 localHit = unit.transform.InverseTransformPoint(hit.point);
            if (!unit.Collider.Overlap(BodyState.Standing, localHit + standOffset, true))
            {
                unit.Animator.Play(UnitAnimationState.Climb);
                unit.Animator.Translate((Vector2)transform.position + localHit + standOffset, unit.Animator.CurrentStateLength, unit.Collider.OnHit);
                return true;
            }
        }
        return false;
    }

    public bool TryDrop()
    {
        if (!unit.Collider.Overlap(BodyState.Standing, Vector2.down * unit.GroundSpring.GroundDistance, true))
        {
            Vector2 horizontalDirection = new Vector2(-unit.Physics.Velocity.x * Time.fixedDeltaTime, 0);
            Debug.DrawRay(transform.position + Vector3.down * unit.GroundSpring.GroundDistance + Vector3.right * unit.Collider.Info[BodyState.Standing].Width * 0.5f * -Mathf.Sign(unit.Physics.Velocity.x), horizontalDirection, Color.magenta);
            Debug.Break();
            return true;
        }
        return false;
    }
}
