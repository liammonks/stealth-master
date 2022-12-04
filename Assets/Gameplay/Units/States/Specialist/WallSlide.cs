using UnityEngine;

namespace States
{
    public class WallSlide : BaseState
    {
        public WallSlide(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            unit.UpdateFacingDirection = false;
            unit.Physics.enabled = true;
            unit.GroundSpring.enabled = false;
            unit.WallSpring.enabled = true;

            unit.Animator.Play(UnitAnimationState.WallSlide);
            return UnitState.WallSlide;
        }
        
        public override UnitState Execute()
        {
            Vector2 velocity = unit.Physics.Velocity;

            // Allow player to push towards movement speed while falling
            if (velocity.y <= 0.0f && Mathf.Abs(velocity.x) < unit.Settings.walkSpeed)
            {
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * DeltaTime;
                unit.Physics.Velocity = velocity;
            }
            // Execute Idle
            if (unit.GroundSpring.Intersecting)
            {
                return UnitState.Idle;
            }
            // Execute Fall
            if (!unit.WallSpring.Intersecting)
            {
                return UnitState.Fall;
            }
            // Grab Ledge
            if (velocity.y <= 0.0f)
            {
                unit.GroundSpring.enabled = true;
                unit.UpdateFacingDirection = true;
                if (unit.StateMachine.GetLastExecutionTime(UnitState.LedgeGrab) >= 0.2f)
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
            unit.UpdateFacingDirection = true;
            unit.GroundSpring.enabled = true;
        }
    }
}