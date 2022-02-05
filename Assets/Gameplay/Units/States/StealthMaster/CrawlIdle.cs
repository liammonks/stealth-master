using UnityEngine;

namespace States.StealthMaster
{
    public class CrawlIdle : BaseState
    {
        private bool toIdle = false;
        private float transitionDuration = 0.0f;

        public CrawlIdle(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            transitionDuration = 0.0f;
            data.isStanding = false;
            data.animator.Play("Crawl_Idle");
            return UnitState.CrawlIdle;
        }
        
        public override UnitState Execute()
        {
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
            if (Mathf.Abs(data.rb.velocity.x) > 0.1f)
            {
                return UnitState.Crawl;
            }

            // Return to Idle
            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                data.ApplyDrag(data.stats.groundDrag);
                if (transitionDuration == 0.0f) return UnitState.Idle;
            }
            else if (!data.input.crawling && data.isGrounded)
            {
                Vector2 standOffset = data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f);
                if (StateManager.CanStand(data, standOffset))
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
            
            if (!data.isGrounded)
            {
                return UnitState.Dive;
            }
            
            StateManager.UpdateFacing(data);
            return UnitState.CrawlIdle;
        }
    }
}