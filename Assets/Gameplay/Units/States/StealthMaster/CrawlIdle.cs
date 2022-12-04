using UnityEngine;

namespace States.StealthMaster
{
    public class CrawlIdle : BaseState
    {
        private bool toIdle = false;
        private float transitionDuration = 0.0f;

        public CrawlIdle(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            transitionDuration = 0.0f;
            unit.Animator.Play(UnitAnimationState.Crawl_Idle);
            return UnitState.CrawlIdle;
        }
        
        public override UnitState Execute()
        {
            // Apply movement input
            if (unit.Input.Movement != 0)
            {
                Vector2 velocity = unit.Physics.Velocity;
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * unit.Settings.groundAcceleration * DeltaTime;
                unit.Physics.Velocity = velocity;
                unit.Physics.SkipDrag();
            }

            // Execute Crawl
            if (Mathf.Abs(unit.Physics.Velocity.x) > unit.Settings.walkSpeed * 0.5f)
            {
                return UnitState.Crawl;
            }

            // Transition to Idle
            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);
                if (transitionDuration == 0.0f) return UnitState.Idle;
            }
            else if (!unit.Input.Crawling && unit.StateMachine.CanStand())
            {
                unit.Animator.Play(UnitAnimationState.CrawlToStand);
                transitionDuration = unit.Animator.CurrentStateLength;
                unit.SetBodyState(BodyState.Standing, transitionDuration);
                toIdle = true;
            }

            // Execute Dive when falling
            if (!unit.GroundSpring.Intersecting)
            {
                return UnitState.Dive;
            }
            
            return UnitState.CrawlIdle;
        }

        public override void Deinitialise()
        {
            
        }

    }
}