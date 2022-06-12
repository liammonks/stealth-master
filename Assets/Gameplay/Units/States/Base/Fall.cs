using UnityEngine;

namespace States
{
    public class Fall : BaseState
    {
        private float stateDuration;

        public Fall(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            unit.Animator.Play(UnitAnimationState.Fall);
            stateDuration = 0.0f;
            return UnitState.Fall;
        }
        
        public override UnitState Execute()
        {
            stateDuration += DeltaTime;
            Vector2 velocity = unit.Physics.Velocity;
            if (velocity.y <= 0)
            {
                unit.GroundSpring.enabled = true;
            }

            // Allow player to push towards movement speed while in the air
            if (unit.Input.Movement != 0 && !unit.GroundSpring.Slipping && Mathf.Abs(velocity.x) < unit.Settings.walkSpeed)
            {
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration;
                unit.Physics.SetVelocity(velocity);
            }
            else
            {
                unit.Physics.ApplyDrag(unit.Settings.airDrag);
            }

            // Return to ground
            if (unit.GroundSpring.Grounded)
            {
                return Mathf.Abs(velocity.x) > (unit.Settings.walkSpeed * 0.5f) ? UnitState.Run : UnitState.Idle;
            }
            
            // Jump (Kyote Time)
            if (unit.Input.Jumping && stateDuration <= UnitInput.KyoteTime)
            {
                return UnitState.Jump;
            }
            
            return UnitState.Fall;
        }

        public override void Deinitialise()
        {

        }
    }
}