using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace States
{
    public class Jump : BaseState
    {
        public Jump(UnitData a_data) : base(a_data) { }

        public override UnitState Initialise()
        {
            Vector2 velocity = data.rb.velocity;
            // Reset jump input
            data.input.jumpRequestTime = -1;
            data.animator.Play("Jump");
            data.animator.UpdateState();
            data.t = data.animator.GetState().length;
            data.isStanding = true;
            data.groundSpringActive = false;
            velocity.y = data.previousState == UnitState.LedgeGrab ? data.stats.wallJumpForce.y : data.stats.jumpForce;

            if (data.attatchedRB)
            {
                velocity += data.attatchedRB.velocity;
                data.attatchedRB = null;
            }
            data.rb.velocity = velocity;
            return UnitState.Jump;
        }
        
        public override UnitState Execute()
        {
            Vector2 velocity = data.rb.velocity;
            data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);

            // Allow player to push towards movement speed while in the air
            if (Mathf.Abs(data.rb.velocity.x) < data.stats.runSpeed)
            {
                float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
            }

            // End of jump animation
            if (data.t == 0.0f)
            {
                data.groundSpringActive = true;
                return data.isGrounded ? UnitState.Idle : UnitState.Fall;
            }

            data.rb.velocity = velocity;
            StateManager.UpdateFacing(data);
            return UnitState.Jump;
        }

    }
}