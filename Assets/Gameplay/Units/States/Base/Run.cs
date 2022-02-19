using UnityEngine;

namespace States
{
    public class Run : BaseState
    {
        public Run(UnitData a_data) : base(a_data) { }

        public override UnitState Initialise()
        {
            data.animator.Play(UnitAnimatorLayer.Body, "Run");
            data.isStanding = true;
            return UnitState.Run;
        }
        
        public override UnitState Execute()
        {
            if (Mathf.Abs(data.rb.velocity.x) < data.stats.runSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * data.stats.groundAcceleration;
                data.rb.velocity = velocity;
            }
            else
            {
                // Apply drag when faster than run speed
                data.ApplyDrag(data.stats.groundDrag);
            }
            
            // Execute Jump
            if (data.input.jumpQueued)
            {
                return UnitState.Jump;
            }
            // Execute Fall
            if (!data.isGrounded)
            {
                return UnitState.Fall;
            }
            // Return to Idle when below walk speed
            if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed * 0.5f)
            {
                return UnitState.Idle;
            }
            
            StateManager.UpdateFacing(data);
            return UnitState.Run;
        }

    }
}