using UnityEngine;

namespace States.StealthMaster
{
    public class WallJump : BaseState
    {
        protected float transitionDuration;

        public WallJump(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            unit.FacingRight = !unit.FacingRight;

            unit.Animator.Play(UnitAnimationState.WallJump);
            transitionDuration = unit.Animator.CurrentStateLength;
            unit.Physics.SetVelocity(new Vector2((unit.FacingRight ? 1 : -1) * unit.Settings.wallJumpForce.x, unit.Settings.wallJumpForce.y));

            return UnitState.WallJump;
        }
        
        public override UnitState Execute()
        {
            transitionDuration = Mathf.Max(0, transitionDuration - DeltaTime);
            Vector2 velocity = unit.Physics.Velocity;

            // Execute Fall
            if (transitionDuration == 0.0f && velocity.y < 0)
            {
                return UnitState.Fall;
            }
            // Try grabbing another ledge
            if (Mathf.Abs(velocity.x) >= unit.Settings.walkSpeed * 0.5f)
            {
                if (unit.StateMachine.TryLedgeGrab())
                {
                    return UnitState.LedgeGrab;
                }
            }
            // Execute Dive
            if (unit.Input.Crawling)
            {
                return UnitState.Dive;
            }
            // Wall Slide
            if (unit.StateMachine.FacingWall())
            {
                return UnitState.WallSlide;
            }
            
            return UnitState.WallJump;
        }

        public override void Deinitialise()
        {
            
        }
    }
}