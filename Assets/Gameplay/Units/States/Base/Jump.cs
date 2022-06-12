using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace States
{
    public class Jump : BaseState
    {
        private float jumpDuration = 0;

        public Jump(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            // Reset jump input
            unit.Input.Jumping = false;
            unit.Animator.Play(UnitAnimationState.Jump);
            jumpDuration = unit.Animator.CurrentStateLength;
            unit.GroundSpring.enabled = false;

            Vector2 velocity = unit.Physics.Velocity;
            velocity.y = unit.StateMachine.PreviousState == UnitState.LedgeGrab ? unit.Settings.wallJumpForce.y : unit.Settings.jumpForce;

            if (unit.GroundSpring.attachedObject)
            {
                velocity += unit.GroundSpring.attachedObject.Velocity;
                unit.GroundSpring.attachedObject = null;
            }

            unit.Physics.SetVelocity(velocity);
            return UnitState.Jump;
        }
        
        public override UnitState Execute()
        {
            jumpDuration = Mathf.Max(0.0f, jumpDuration - DeltaTime);

            // Allow player to push towards movement speed while in the air
            if (unit.Input.Movement != 0)
            {
                Vector2 velocity = unit.Physics.Velocity;
                if (Mathf.Abs(velocity.x) < unit.Settings.runSpeed)
                {
                    float desiredSpeed = (unit.Input.Running ? unit.Settings.runSpeed : unit.Settings.walkSpeed) * unit.Input.Movement;
                    float deltaSpeedRequired = desiredSpeed - velocity.x;
                    velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * DeltaTime;
                    unit.Physics.SetVelocity(velocity);
                }
            }
            else
            {
                unit.Physics.ApplyDrag(unit.Settings.airDrag);
            }

            // End of jump animation
            if (jumpDuration == 0.0f)
            {
                unit.GroundSpring.enabled = true;
                return unit.GroundSpring.Grounded ? UnitState.Idle : UnitState.Fall;
            }

            return UnitState.Jump;
        }

        public override void Deinitialise()
        {
            
        }
    }
}