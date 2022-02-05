using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StateManager
{
    public static void UpdateFacing(UnitData data)
    {
        data.animator.SetVelocity(data.rb.velocity.x);

        if (data.rb.velocity.x > 0.1f) { data.isFacingRight = true; }
        else if (data.rb.velocity.x < -0.1f) { data.isFacingRight = false; }
        else if (data.input.movement > 0.0f) { data.isFacingRight = true; }
        else if (data.input.movement < 0.0f) { data.isFacingRight = false; }
        data.animator.SetFacing(data.isFacingRight);
    }

    public static bool FacingWall(UnitData data)
    {
        const float detectionDepth = 0.1f;
        float bodyWidth = data.isStanding ? data.stats.standingHalfWidth : data.stats.crawlingHalfWidth;
        RaycastHit2D wallHit = Physics2D.BoxCast(
            data.rb.position + (Vector2.up * data.stats.standingScale.y * 0.25f),
            new Vector2(detectionDepth, data.stats.standingScale.y * 0.5f),
            data.rb.rotation,
            data.isFacingRight ? data.rb.transform.right : -data.rb.transform.right,
            bodyWidth,
            Unit.CollisionMask
        );
        if (wallHit)
        {
            ExtDebug.DrawBoxCastOnHit(
                data.rb.position + (Vector2.up * data.stats.standingScale.y * 0.25f),
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

    public static bool CanStand(UnitData data, Vector2 offset)
    {
        RaycastHit2D hit = Physics2D.BoxCast(data.rb.position + offset, data.stats.standingScale * 0.9f, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + offset, (data.stats.standingScale * 0.9f) * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
        return !hit;
    }
}
