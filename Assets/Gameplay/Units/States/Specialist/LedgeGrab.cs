using UnityEngine;

namespace States
{
    public class LedgeGrab : BaseState
    {
        protected const float inputLockDuration = 0.2f;
        protected const float slideLerpDuration = 0.5f;
        protected const float standardLerpDuration = 0.2f;
        protected float lerpDuration;

        public LedgeGrab(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            unit.Physics.enabled = false;
            // Check if the ledge wall extends down to feet
            //RaycastHit2D feetHit = Physics2D.Raycast(
            //    data.target + (Vector2.down * data.stats.standingScale * 0.4f),
            //    data.isFacingRight ? Vector2.right : Vector2.left,
            //    data.stats.standingScale.x * 0.6f,
            //    Unit.CollisionMask
            //);
            //Debug.DrawRay(
            //    data.target + (Vector2.down * data.stats.standingScale * 0.4f),
            //    (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.6f,
            //    feetHit ? Color.green : Color.red,
            //    1.0f
            //);
            //data.groundSpringActive = false;
            //data.animator.Play(UnitAnimatorLayer.Body, feetHit ? "LedgeGrab" : "LedgeGrab_Hang");
            //lerpDuration = data.previousState == UnitState.Slide ? slideLerpDuration : standardLerpDuration;
            //data.isStanding = true;
            return UnitState.LedgeGrab;
        }
        
        public override UnitState Execute()
        {
            //data.rb.rotation = 0.0f;
            //data.rb.velocity = Vector2.zero;
            //data.isGrounded = true;

            //if (data.attatchedRB)
            //{
            //    data.target += data.attatchedRB.velocity * Time.fixedDeltaTime;
            //}

            //// Move player to target position
            //if (data.stateDuration < lerpDuration)
            //{
            //    data.rb.position = Vector2.Lerp(data.rb.position, data.target, data.stateDuration / lerpDuration);
            //}
            //else
            //{
            //    data.rb.position = data.target;
            //}
            
            //if (data.stateDuration >= inputLockDuration)
            //{
            //    // Climb Right
            //    if (data.isFacingRight && data.input.movement > 0)
            //    {
            //        if (StateManager.CanClimb(data))
            //        {
            //            return UnitState.Climb;
            //        }
            //    }
            //    // Climb Left
            //    if (!data.isFacingRight && data.input.movement < 0)
            //    {
            //        if (StateManager.CanClimb(data))
            //        {
            //            return UnitState.Climb;
            //        }
            //    }
            //    // Drop
            //    if (data.input.crawling)
            //    {
            //        data.input.crawling = false;
            //        data.input.crawlRequestTime = -1;
            //        return UnitState.WallSlide;
            //    }
            //}

            return UnitState.LedgeGrab;
        }

        public override void Deinitialise()
        {
            
        }
    }
}