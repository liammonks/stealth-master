using UnityEngine;

namespace States
{
    public class Idle : BaseState
    {
        public Idle(UnitData a_data) : base(a_data) { }

        public override UnitState Initialise()
        {
            data.animator.Play(UnitAnimatorLayer.Body, "Idle");
            data.isStanding = true;
            return UnitState.Idle;
        }
        
        public override UnitState Execute()
        {
            data.ApplyDrag(data.stats.groundDrag);
            
            // Move to desired speed
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
            
            // Execute Run
            if (data.input.movement != 0 && Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed * 0.5f)
            {
                return UnitState.Run;
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

            StateManager.UpdateFacing(data);
            return UnitState.Idle;
        }

    }
}