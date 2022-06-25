using UnityEngine;

namespace States
{
    public class WallSlide : BaseState
    {
        protected float stateDuration = 0.0f;

        public WallSlide(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            unit.Physics.enabled = true;
            unit.GroundSpring.enabled = false;

            stateDuration = 0.0f;

            unit.Animator.Play(UnitAnimationState.WallSlide);
            unit.Physics.SetVelocity(new Vector2(unit.FacingRight ? 5f : -5f, unit.Physics.Velocity.y));
            return UnitState.WallSlide;
        }
        
        public override UnitState Execute()
        {
            stateDuration += DeltaTime;
            Vector2 velocity = unit.Physics.Velocity;
            // Allow player to push towards movement speed while in the air
            if (Mathf.Abs(velocity.x) < unit.Settings.walkSpeed)
            {
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration;
                unit.Physics.SetVelocity(velocity);
            }
            // Execute Idle
            if (unit.GroundSpring.Grounded)
            {
                return UnitState.Idle;
            }
            // Execute Fall
            if (stateDuration >= 0.5f && !unit.StateMachine.FacingWall())
            {
                return UnitState.Fall;
            }
            // Grab Ledge
            if (velocity.y <= 0.0f)
            {
                unit.GroundSpring.enabled = true;

                if (unit.StateMachine.TryLedgeGrab())
                {
                    return UnitState.LedgeGrab;
                }
            }

            return UnitState.WallSlide;
        }

        public override void Deinitialise()
        {
            
        }
    }
}