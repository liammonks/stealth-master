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
            unit.WallSpring.enabled = true;
            stateDuration = 0.0f;

            unit.Animator.Play(UnitAnimationState.WallSlide);
            return UnitState.WallSlide;
        }
        
        public override UnitState Execute()
        {
            stateDuration += DeltaTime;
            Vector2 velocity = unit.Physics.Velocity;

            if (Mathf.Abs(velocity.x) < unit.Settings.walkSpeed)
            {
                // Allow player to push towards movement speed while in the air
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * DeltaTime;
                unit.Physics.SetVelocity(velocity);
            }
            // Execute Idle
            if (unit.GroundSpring.Intersecting)
            {
                return UnitState.Idle;
            }
            // Execute Fall
            if (stateDuration >= 0.5f && !unit.WallSpring.Intersecting)
            {
                return UnitState.Fall;
            }
            // Grab Ledge
            if (velocity.y <= 0.0f && unit.StateMachine.GetLastExecutionTime(UnitState.LedgeGrab) >= 0.2f)
            {
                unit.GroundSpring.enabled = true;

                if (unit.StateMachine.PreviousState != UnitState.LedgeGrab || stateDuration >= 0.2f)
                {
                    if (unit.StateMachine.TryLedgeGrab())
                    {
                        return UnitState.LedgeGrab;
                    }
                }
            }

            return UnitState.WallSlide;
        }

        public override void Deinitialise()
        {
            
        }
    }
}