using UnityEngine;

namespace States.StealthMaster
{
    public class Dive : BaseState
    {
        protected bool toStand = false;
        protected float groundSpringPrevention = 0.0f;
        protected float transitionDuration = 0.0f;

        public Dive(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            toStand = false;
            data.animator.Play(UnitAnimatorLayer.Body, "Dive");
            data.isStanding = false;
            // Boost when diving from jump
            if (data.previousState == UnitState.Jump || data.previousState == UnitState.WallJump)
            {
                //data.rb.velocity *= data.stats.diveVelocityMultiplier;
                Vector2 velocity = data.rb.velocity;
                velocity.x += 5 * Mathf.Sign(data.rb.velocity.x);
                velocity.y += 2 * Mathf.Sign(data.rb.velocity.y);
                data.rb.velocity = velocity;
            }
            // Set timer to stop ground spring
            data.groundSpringActive = false;
            groundSpringPrevention = 0.2f;
            return UnitState.Dive;
        }
        
        public override UnitState Execute()
        {
            if (toStand)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                // Execute Jump (only 100ms before returning idle)
                if (transitionDuration < 0.1f && data.input.jumpQueued && data.isGrounded)
                {
                    return UnitState.Jump;
                }
                // Execute Idle
                if (transitionDuration == 0.0f)
                {
                    return UnitState.Idle;
                }
                data.ApplyDrag(data.stats.groundDrag);
                return UnitState.Dive;
            }
            
            // Allow player to push towards movement speed while in the air
            if (!data.isSlipping && Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = data.stats.walkSpeed * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
                data.rb.velocity = velocity;
            }
            
            // Re-enable ground spring after delay
            if (!data.groundSpringActive)
            {
                groundSpringPrevention = Mathf.Max(0.0f, groundSpringPrevention - Time.fixedDeltaTime);
                if (groundSpringPrevention == 0.0f)
                {
                    data.groundSpringActive = true;
                }
            }
            // Check Landed
            else if (data.isGrounded)
            {
                // Not crawling, stand up
                if (!data.input.crawling)
                {
                    Vector2 offset = data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f);
                    if (StateManager.CanStand(data, offset))
                    {
                        // Execute animation transition
                        data.animator.Play(UnitAnimatorLayer.Body, Mathf.Abs(data.rb.velocity.x) > data.stats.runSpeed ? "DiveFlip" : "CrawlToStand");
                        // Update animator to transition to relevant state
                        data.animator.UpdateState();
                        transitionDuration = data.animator.GetState().length;
                        data.isStanding = true;
                        data.LockGadget();
                        toStand = true;
                    }
                }
                else
                {
                    // Landed but still crawling
                    if (Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed)
                    {
                        // Execute Slide
                        data.animator.Play(UnitAnimatorLayer.Body, "BellySlide");
                        return UnitState.Slide;
                    }
                    else
                    {
                        // Execute Crawl
                        data.animator.Play(UnitAnimatorLayer.Body, "Crawl_Idle");
                        return UnitState.Crawl;
                    }
                }
            }
            
            data.ApplyDrag(data.stats.airDrag);
            StateManager.UpdateFacing(data);
            return UnitState.Dive;
        }
    }
}