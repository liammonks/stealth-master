using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Null,
    Idle,
    Run,
    Crawl,
    Slide,
    Dive,
    Jump,
    VaultOver,
    VaultOn,
    Fall,
    CrawlIdle,
    LedgeGrab,
    WallJump,
    Climb,
    WallSlide,
    Melee,
    JumpMelee,
    HitImpact,
    Launched,
    Bite,
    Alert,
    Pounce,
    Crouch
}

public static class StateManager
{
    public static void UpdateFacing(Unit unit)
    {
        unit.animator.SetVelocity(data.rb.velocity.magnitude * Mathf.Sign(data.rb.velocity.x));

        if (unit.rb.velocity.x > 0.1f) { unit.animator.SetFacing(true); }
        else if (unit.rb.velocity.x < -0.1f) { unit.animator.SetFacing(false); }
        else if (unit.input.movement > 0.0f) { unit.animator.SetFacing(true); }
        else if (unit.input.movement < 0.0f) { unit.animator.SetFacing(false); }
    }

    public static bool FacingWall(Unit unit)
    {
        const float detectionDepth = 0.1f;
        float bodyWidth = data.isStanding ? data.stats.standingHalfWidth : data.stats.crawlingHalfWidth;
        RaycastHit2D wallHit = Physics2D.BoxCast(
            data.rb.position,// + (Vector2.up * data.stats.standingScale.y * 0.25f),
            new Vector2(detectionDepth, data.stats.standingScale.y * 0.5f),
            data.rb.rotation,
            data.isFacingRight ? data.rb.transform.right : -data.rb.transform.right,
            bodyWidth,
            Unit.CollisionMask
        );
        if (wallHit)
        {
            ExtDebug.DrawBoxCastOnHit(
                data.rb.position,// + (Vector2.up * data.stats.standingScale.y * 0.25f),
                new Vector2(detectionDepth, data.stats.standingScale.y * 0.5f) * 0.5f,
                Quaternion.Euler(0, 0, data.rb.rotation),
                data.isFacingRight ? data.rb.transform.right : -data.rb.transform.right,
                wallHit.distance,
                Color.green
            );
        }
        return wallHit;
    }

    public static bool CanCrawl(UnitData data)
    {
        const float sideCheckOffset = 0.1f;
        float heightOffset = -data.stats.standingHalfHeight + data.stats.crawlingHalfHeight + 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset), data.stats.crawlingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset), data.stats.crawlingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
        if (hit)
        {
            // Check left side
            hit = Physics2D.BoxCast(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + (-(Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
            ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + (-(Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
            if (hit)
            {
                // Check Right side
                hit = Physics2D.BoxCast(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + ((Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
                ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + ((Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
            }
        }
        return !hit;
    }

    public static bool CanStand(Unit unit, Vector2 offset)
    {
        RaycastHit2D hit = Physics2D.BoxCast(data.rb.position + offset, data.stats.standingScale * 0.9f, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + offset, (data.stats.standingScale * 0.9f) * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
        return !hit;
    }

    public static UnitState TryDrop(Unit unit)
    {
        const float boxDepth = 0.1f;
        const float scanDepth = 0.5f;
        const float scanDepthInterval = 0.01f;
        float castDist = data.stats.standingScale.x;

        RaycastHit2D dropHit = Physics2D.BoxCast(
            data.rb.position,
            new Vector2(data.stats.standingScale.x, boxDepth),
            0,
            Vector2.down,
            data.stats.standingScale.y - (boxDepth * 0.05f),
            Unit.CollisionMask
        );
        //ExtDebug.DrawBox(
        //    data.rb.position + (Vector2.down * data.stats.standingScale.y * 0.5f),
        //    data.stats.standingScale * 0.5f,
        //    Quaternion.identity,
        //    dropHit ? Color.green : Color.red
        //);

        if (!dropHit)
        {
            float depth = 0.0f;
            while (depth <= scanDepth)
            {
                RaycastHit2D ledgeHit = Physics2D.Raycast(
                    data.rb.position + (Vector2.down * (data.stats.crawlingHalfHeight + depth)),
                    data.isFacingRight ? Vector2.left : Vector2.right,
                    data.stats.standingScale.x,
                    Unit.CollisionMask
                );
                Debug.DrawRay(
                    data.rb.position + (Vector2.down * (data.stats.crawlingHalfHeight + depth)),
                    (data.isFacingRight ? Vector2.left : Vector2.right) * (ledgeHit ? ledgeHit.distance : data.stats.standingScale.x),
                    ledgeHit ? Color.green : Color.red
                );
                if (ledgeHit)
                {
                    data.target = ledgeHit.point + (data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x + Vector2.up * data.stats.climbGrabOffset.y;
                    data.isFacingRight = !data.isFacingRight;
                    data.animator.SetFacing(data.isFacingRight);
                    return UnitState.LedgeGrab;
                }
                depth += scanDepthInterval;
            }
        }

        return UnitState.Null;
    }

    public static UnitState TryVault(UnitData data)
    {
        const float nearHitBuffer = 0.25f;
        RaycastHit2D nearHit = Physics2D.BoxCast(
            data.rb.position + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)) + (-(Vector2)data.rb.transform.up * (data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f),
            new Vector2(data.stats.vaultGrabDistance - nearHitBuffer, data.stats.maxVaultHeight - data.stats.minVaultHeight),
            data.rb.rotation,
            data.isFacingRight ? Vector2.right : Vector2.left,
            data.stats.vaultGrabDistance * 0.5f,
            Unit.CollisionMask
        );
        ExtDebug.DrawBoxCastOnHit(
            data.rb.position + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)) + (-(Vector2)data.rb.transform.up * (data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f),
            new Vector2(data.stats.vaultGrabDistance - nearHitBuffer, data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f,
            Quaternion.Euler(0, 0, data.rb.rotation),
            data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right,
            data.stats.vaultGrabDistance * 0.5f,
            nearHit ? Color.green : Color.red
        );
        if (nearHit) { return UnitState.Null; }

        RaycastHit2D vaultHit = Physics2D.Raycast(
            data.rb.position + ((data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right) * data.stats.vaultGrabDistance) + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
            -(Vector2)data.rb.transform.up,
            data.stats.maxVaultHeight - data.stats.minVaultHeight
        );
        Debug.DrawRay(
            data.rb.position + ((data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right) * data.stats.vaultGrabDistance) + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
            -(Vector2)data.rb.transform.up * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
            Color.red
        );
        if (vaultHit)
        {
            Debug.DrawRay(
                data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultGrabDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                Vector2.down * vaultHit.distance,
                Color.green,
                data.stats.vaultDuration
            );
            // Dont vault if surface is not flat
            if (Vector2.Dot(Vector2.up, vaultHit.normal) <= 0.9f)
            {
                Debug.DrawRay(vaultHit.point, vaultHit.normal, Color.red, data.stats.vaultDuration);
                return UnitState.Null;
            }
            // Dont vault if ray is inside a collider
            if (vaultHit.distance > 0.0f)
            {
                // Vault over object or on top of it
                RaycastHit2D landingZoneHit = Physics2D.Raycast(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                    Vector2.down,
                    data.stats.maxVaultHeight - data.stats.minVaultHeight
                );
                Debug.DrawRay(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                    Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                    Color.red,
                    data.stats.vaultDuration
                );
                if (landingZoneHit)
                {
                    // Landing zone obstruction, try to vault on top of the object
                    Debug.DrawRay(
                        data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                        Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                        Color.green,
                        data.stats.vaultDuration
                    );
                    data.target = ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.up * data.stats.standingHalfHeight * 0.5f);
                    // Check if the target area is clear for standing
                    landingZoneHit = Physics2D.BoxCast(
                        data.rb.position + data.target + (Vector2.up * data.stats.standingHalfHeight * 0.5f) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f),
                        data.stats.standingScale,
                        0,
                        Vector2.zero,
                        0,
                        Unit.CollisionMask
                    );
                    ExtDebug.DrawBox(
                        data.rb.position + data.target + (Vector2.up * data.stats.standingHalfHeight * 0.5f) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f),
                        data.stats.standingScale * 0.5f,
                        Quaternion.identity,
                        landingZoneHit ? Color.green : Color.red,
                        data.stats.vaultDuration
                    );
                    if (!landingZoneHit)
                    {
                        return UnitState.VaultOn;
                    }
                }
                else
                {
                    const float groundClearance = 0.25f;
                    // Landing zone clear, try to vault over
                    data.target = ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance);
                    landingZoneHit = Physics2D.BoxCast(
                        data.rb.position + data.target + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f) + (Vector2.up * groundClearance),
                        new Vector2(data.stats.standingScale.x, data.stats.standingScale.y - groundClearance),
                        0,
                        Vector2.zero,
                        0,
                        Unit.CollisionMask
                    );
                    ExtDebug.DrawBox(
                        data.rb.position + data.target + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f) + (Vector2.up * groundClearance),
                        new Vector2(data.stats.standingScale.x, data.stats.standingScale.y - groundClearance) * 0.5f,
                        Quaternion.identity,
                        landingZoneHit ? Color.green : Color.red,
                        data.stats.vaultDuration
                    );
                    if (!landingZoneHit)
                    {
                        return UnitState.VaultOver;
                    }
                }
            }
        }
        return UnitState.Null;
    }

    public static UnitState TryLedgeGrab(UnitData data)
    {
        float minLedgeThickness = 0.1f;
        float scanHeight = data.stats.maxClimbHeight - data.stats.minClimbHeight;
        float scanHeightInterval = 0.01f;
        const float boxDepth = 0.1f;
        float castDist = data.stats.climbGrabDistance / Mathf.Max(Mathf.Pow(data.rb.rotation, 0.25f), 1.0f);
        RaycastHit2D climbHit = Physics2D.BoxCast(
            data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
            new Vector2(boxDepth, scanHeight),
            0,
            data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right,
            castDist - (boxDepth * 0.05f),
            Unit.CollisionMask
        );
        //ExtDebug.DrawBox(
        //    data.rb.position + ((data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right) * castDist * 0.5f) - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
        //    new Vector2(castDist, scanHeight) * 0.5f,
        //    Quaternion.identity,
        //    Color.red
        //);

        while (climbHit && scanHeight > (scanHeightInterval * 2))
        {
            scanHeight -= scanHeightInterval;
            RaycastHit2D scanHit = Physics2D.BoxCast(
                data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
                new Vector2(boxDepth, scanHeight),
                0,
                data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right,
                castDist - (boxDepth * 0.05f),
                Unit.CollisionMask
            );
            //Debug.DrawLine(
            //    data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
            //    scanHit.point,
            //    Color.red,
            //    data.stats.climbDuration
            //);

            if (scanHit && scanHit.distance <= climbHit.distance + minLedgeThickness)
            {
                // Continue scanning down until we do not hit
            }
            else
            {
                //Debug.DrawLine(
                //    data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
                //    climbHit.point,
                //    Color.green,
                //    data.stats.climbDuration
                //);
                Vector2 normal = (data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f)))) - climbHit.point;
                if (Vector2.Dot(normal, Vector2.up) > 0.05f)
                {
                    data.target = climbHit.point + (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.climbGrabOffset.x + Vector2.up * data.stats.climbGrabOffset.y;
                    data.attatchedRB = climbHit.rigidbody;
                    return UnitState.LedgeGrab;
                }
                else
                {
                    return UnitState.Null;
                }
            }
            climbHit = scanHit;
        }
        return UnitState.Null;
    }

    public static bool CanClimb(UnitData data)
    {
        Vector2 target = data.rb.position + (Vector2.up * (data.stats.standingHalfHeight + 0.05f - data.stats.climbGrabOffset.y)) + ((data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x);
        RaycastHit2D hit = Physics2D.BoxCast(target, (data.stats.standingScale * 0.9f), data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(target, (data.stats.standingScale * 0.9f) * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green, data.stats.climbDuration);
        return !hit;
    }
}
