using UnityEngine;

namespace States
{
    public class Crawl : BaseState
    {
        protected bool toIdle = false;
        protected float transitionDuration = 0.0f;

        public Crawl(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            data.isStanding = false;
            data.animator.Play("Crawl");
            return UnitState.Crawl;
        }
        
        public override UnitState Execute()
        {
            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                data.ApplyDrag(data.stats.groundDrag);
                if (transitionDuration == 0.0f) return Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed * 0.5f ? UnitState.Run : UnitState.Idle;
                return UnitState.Crawl;
            }
            
            // Apply movement input
            if (data.isGrounded && data.rb.velocity.x < data.stats.walkSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = data.stats.walkSpeed * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * data.stats.groundAcceleration;
                data.rb.velocity = velocity;
            }

            // Return to CrawlIdle
            if (Mathf.Abs(data.rb.velocity.x) < 0.1f)
            {
                return UnitState.CrawlIdle;
            }
            
            // Return to Idle
            if (!data.input.crawling && data.isGrounded)
            {
                Vector2 offset = data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f);
                if (StateManager.CanStand(data, offset))
                {
                    // Execute animation transition
                    data.animator.Play("CrawlToStand");
                    // Update animator to transition to relevant state
                    data.animator.UpdateState();
                    transitionDuration = data.animator.GetState().length;
                    data.isStanding = true;
                    toIdle = true;
                }
            }
            
            StateManager.UpdateFacing(data);
            return UnitState.Crawl;
        }
    }
}