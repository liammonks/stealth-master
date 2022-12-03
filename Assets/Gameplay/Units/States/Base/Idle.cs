using UnityEngine;

namespace States
{
    public class Idle : BaseState
    {
        public Idle(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            unit.Animator.Play(UnitAnimationState.Idle);
            return UnitState.Idle;
        }
        
        public override UnitState Execute()
        {
            // Move to desired speed
            Vector2 velocity = unit.Physics.velocity;

            if (unit.Input.Movement != 0)
            {
                float desiredSpeed = (unit.Input.Running ? unit.Settings.runSpeed : unit.Settings.walkSpeed) * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * unit.Settings.groundAcceleration * DeltaTime;
                unit.Physics.velocity = velocity;

                // Execute Run
                if (Mathf.Abs(velocity.x) > unit.Settings.walkSpeed * 0.5f)
                {
                    return UnitState.Run;
                }
            }
            else
            {
                unit.Physics.drag = unit.Settings.groundDrag;
            }

            // Execute Jump
            if (unit.Input.Jumping)
            {
                return UnitState.Jump;
            }
            // Execute Fall
            if (!unit.GroundSpring.Intersecting)
            {
                return UnitState.Fall;
            }

            return UnitState.Idle;
        }

        public override void Deinitialise()
        {

        }
    }
}