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
            states[currentState].Initialise();
        }
        lastFrameState = currentState;
        currentState = states[currentState].Execute();
    }

    public bool AgainstWall()
    {
        const float edgeBuffer = 0.02f;
        return unit.Collider.Overlap(BodyState.Standing, new Vector2(unit.FacingRight ? edgeBuffer : -edgeBuffer, 0), new Vector2(1.0f, 0.5f), true);
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

    public bool CanVaultOver()
    {
        float vaultCheckHeight = unit.Settings.vaultMaxHeight - unit.Settings.vaultMinHeight;

        // Near check
        Vector2 nearCheckSize = new Vector2(unit.Settings.vaultGrabDistance, vaultCheckHeight);
        Vector2 nearCheckOrigin = new Vector2
        {
            x = (unit.FacingRight ? 1 : -1) * (unit.Settings.vaultGrabDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) - (nearCheckSize.x * 0.5f)) - (unit.Physics.Velocity.x * Time.fixedDeltaTime),
            y = -unit.GroundSpring.GroundDistance + unit.Settings.vaultMinHeight + (vaultCheckHeight * 0.5f)
        };
        Collider2D nearCollider = Physics2D.OverlapBox((Vector2)transform.position + nearCheckOrigin, nearCheckSize, transform.eulerAngles.z, 8);
        DebugExtension.DebugLocalCube(transform, nearCheckSize, nearCollider ? Color.green : Color.red, nearCheckOrigin);
        if (nearCollider) { return false; }

        Vector2 vaultCheckOrigin = new Vector2
        {
            x = unit.FacingRight ? unit.Settings.vaultGrabDistance + (unit.Collider.Info[BodyState.Standing].Width * 0.5f) : -unit.Settings.vaultGrabDistance - (unit.Collider.Info[BodyState.Standing].Width * 0.5f),
            y = -unit.GroundSpring.GroundDistance + unit.Settings.vaultMinHeight + vaultCheckHeight
        };
        Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -transform.up * vaultCheckHeight, Color.blue);
        Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -unit.Physics.Velocity * Time.fixedDeltaTime, Color.blue);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)vaultCheckOrigin, -transform.up, vaultCheckHeight, 8);

        if (hit.collider != null)
        {
            const int iterationCount = 10;
            Vector2 maxOrigin = vaultCheckOrigin - new Vector2(unit.Physics.Velocity.x * Time.fixedDeltaTime, 0);
            for (int i = 1; i < iterationCount; ++i)
            {
                Vector2 iterationOrigin = Vector2.Lerp(vaultCheckOrigin, maxOrigin, (float)i / iterationCount);
                RaycastHit2D iterationHit = Physics2D.Raycast(transform.position + (Vector3)iterationOrigin, -transform.up, vaultCheckHeight, 8);
                if (iterationHit.collider != null) { hit = iterationHit; }
                Debug.DrawRay(transform.position + (Vector3)iterationOrigin, -transform.up * vaultCheckHeight, Color.red);
            }

            //Debug.DrawRay(transform.position + (Vector3)vaultCheckOrigin, -transform.up * hit.distance, Color.green);
            DebugExtension.DebugPoint(hit.point, Color.green, 0.1f);
            Vector2 standOffset = new Vector2
            {
                x = 0,//unit.FacingRight ? unit.Collider.Info[BodyState.Standing].Width : -unit.Collider.Info[BodyState.Standing].Width,
                y = unit.GroundSpring.GroundDistance
            };
            unit.Collider.Overlap(BodyState.Standing, (Vector2)transform.InverseTransformPoint(hit.point) + standOffset, true);
            Debug.Break();
        }
        return false;
    }
}
