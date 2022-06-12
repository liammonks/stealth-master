using UnityEngine;

namespace States.StealthMaster
{
    public class WallJump : BaseState
    {
        protected float transitionDuration;

        public WallJump(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            // Flip facing
            data.isFacingRight = !data.isFacingRight;
            data.animator.SetFacing(data.isFacingRight);
            data.animator.Play(UnitAnimatorLayer.Body, "WallJump");
            data.animator.UpdateState();
            transitionDuration = data.animator.GetState().length;
            data.rb.velocity = (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.wallJumpForce.x +
                                Vector2.up * data.stats.wallJumpForce.y;
            return UnitState.WallJump;
        }
        
        public override UnitState Execute()
        {
            transitionDuration = Mathf.Max(0, transitionDuration - Time.fixedDeltaTime);
            
            // Execute Fall
            if (transitionDuration == 0.0f && data.rb.velocity.y < 0)
            {
                data.animator.Play(UnitAnimatorLayer.Body, "WallJumpFall");
                data.groundSpringActive = true;
                return UnitState.Fall;
            }
            // Try grabbing another ledge
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.walkSpeed * 0.5f)
            {
                UnitState climbState = StateManager.TryLedgeGrab(data);
                if (climbState != UnitState.Null)
                {
                    return climbState;
                }
            }
            // Execute Dive
            if (data.input.crawling)
            {
                return UnitState.Dive;
            }
            // Wall Slide
            if (StateManager.FacingWall(data))
            {
                return UnitState.WallSlide;
            }
            
            StateManager.UpdateFacing(data);
            return UnitState.WallJump;
        }
    }
}