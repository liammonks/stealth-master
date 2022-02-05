using UnityEngine;

namespace States
{
    public class WallSlide : BaseState
    {
        public WallSlide(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            data.animator.Play("WallSlide");
            data.rb.velocity = new Vector2(data.isFacingRight ? 0.5f : -0.5f, data.rb.velocity.y);
            return UnitState.WallSlide;
        }
        
        public override UnitState Execute()
        {
            // Allow player to push towards movement speed while in the air
            if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = data.stats.walkSpeed * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
                data.rb.velocity = velocity;
            }
            // Execute Idle
            if (data.isGrounded)
            {
                return UnitState.Idle;
            }
            // Execute Fall
            if (!StateManager.FacingWall(data))
            {
                return UnitState.Fall;
            }
            // Grab Ledge
            if (data.rb.velocity.y <= 0.0f)
            {
                UnitState climbState = StateManager.TryLedgeGrab(data);
                if (climbState != UnitState.Null)
                {
                    return climbState;
                }
            }
            
            if (data.rb.velocity.y <= 0.0f)
            {
                data.groundSpringActive = true;
            }

            return UnitState.WallSlide;
        }
    }
}