using UnityEngine;

namespace States
{
    public class Crawl : BaseState
    {
        protected bool toIdle = false;
        protected float transitionDuration = 0.0f;

        public Crawl(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            transitionDuration = 0.0f;
            unit.Animator.Play(UnitAnimationState.Crawl);
            return UnitState.Crawl;
        }
        
        public override UnitState Execute()
        {
            Vector2 velocity = unit.Physics.Velocity;

            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);
                if (transitionDuration == 0.0f) return Mathf.Abs(velocity.x) > unit.Settings.walkSpeed * 0.5f ? UnitState.Run : UnitState.Idle;
                return UnitState.Crawl;
            }
            
            // Apply movement input
            if (unit.GroundSpring.Grounded && velocity.x < unit.Settings.walkSpeed)
            {
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * unit.Settings.groundAcceleration * DeltaTime;
                unit.Physics.SetVelocity(velocity);
            }

            // Return to CrawlIdle
            if (Mathf.Abs(velocity.x) < 0.1f)
            {
                return UnitState.CrawlIdle;
            }
            
            // Return to Idle
            if (!unit.Input.Crawling && CanStand())
            {
                unit.Animator.Play(UnitAnimationState.CrawlToStand);
                transitionDuration = unit.Animator.CurrentStateLength;
                unit.SetBodyState(BodyState.Standing, transitionDuration);
                toIdle = true;
            }
            
            return UnitState.Crawl;
        }

        public override void Deinitialise()
        {
            
        }

        private bool CanStand()
        {
            const float edgeBuffer = 0.02f;
            if (!unit.GroundSpring.Grounded) return false;
            float xOffset = (unit.Collider.Info[BodyState.Standing].Width * 0.5f);
            float yOffset = (unit.Collider.Info[BodyState.Standing].Height * 0.5f) - unit.GroundSpring.GroundDistance + edgeBuffer;
            //if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(0, yOffset), true)) { return true; }
            if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(xOffset, yOffset), true)) { return true; }
            //if (!unit.Collider.Overlap(BodyState.Standing, new Vector2(-xOffset, yOffset), true)) { return true; }
            return false;
        }
    }
}