using UnityEngine;

namespace States
{
    public class Fall : BaseState
    {
        private const float kyoteTime = 0.2f;
        private float stateDuration;

        public Fall(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            switch (unit.StateMachine.PreviousState)
            {
                case UnitState.WallJump:
                    unit.Animator.Play(UnitAnimationState.WallJumpFall);
                    break;
                default:
                    unit.Animator.Play(UnitAnimationState.Fall);
                    break;
            }

            stateDuration = 0.0f;
            return UnitState.Fall;
        }
        
        public override UnitState Execute()
        {
            stateDuration += Time.fixedDeltaTime;
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
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * Time.fixedDeltaTime;
                unit.Physics.Velocity = velocity;
                unit.Physics.SkipDrag();
            }

            // Return to ground
            if (unit.GroundSpring.Intersecting)
            {
                return Mathf.Abs(velocity.x) > (unit.Settings.walkSpeed * 0.5f) ? UnitState.Run : UnitState.Idle;
            }
            
            // Jump (Kyote Time)
            if (unit.Input.Jumping && stateDuration <= kyoteTime)
            {
                if (unit.StateMachine.PreviousState == UnitState.Idle || unit.StateMachine.PreviousState == UnitState.Run)
                {
                    return UnitState.Jump;
                }
            }
            
            return UnitState.Fall;
        }

        public override void Deinitialise()
        {

        }
    }
}