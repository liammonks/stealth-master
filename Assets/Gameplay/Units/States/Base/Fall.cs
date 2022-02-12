using UnityEngine;

namespace States
{
    public class Fall : BaseState
    {
        public Fall(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            data.animator.Play("Fall");
            data.isStanding = true;
            return UnitState.Fall;
        }
        
        public override UnitState Execute()
        {
            if (data.rb.velocity.y <= 0)
            {
                data.groundSpringActive = true;
            }

            // Allow player to push towards movement speed while in the air
            if (!data.isSlipping && Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = data.stats.walkSpeed * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
                data.rb.velocity = velocity;
            }

            // Return to ground
            if (data.isGrounded)
            {
                return Mathf.Abs(data.rb.velocity.x) > 0.1f ? UnitState.Run : UnitState.Idle;
            }
            
            // Jump (Kyote Time)
            if (data.input.jumpQueued && data.canJump)
            {
                return UnitState.Jump;
            }
            
            StateManager.UpdateFacing(data);
            return UnitState.Fall;
        }

    }
}