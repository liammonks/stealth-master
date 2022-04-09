using UnityEngine;

namespace States
{
    public class Slide : BaseState
    {
        protected bool toIdle = false;
        protected float transitionDuration = 0.0f;

        public Slide(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            transitionDuration = 0.0f;
            data.isStanding = false;
            if (data.previousState != UnitState.Dive)
            {
                data.rb.velocity *= data.stats.slideVelocityMultiplier;
            }
            return UnitState.Slide;
        }
        
        public override UnitState Execute()
        {
            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                data.ApplyDrag(data.stats.groundDrag);
                // Execute Jump (only 100ms before returning idle)
                if (data.t < 0.1f && data.input.jumpQueued) return UnitState.Jump;
                // Execute Run / Idle
                if (data.t == 0.0f) return Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed * 0.5f ? UnitState.Run : UnitState.Idle;
            }
            
            // Transition Idle
            if (!data.input.crawling && data.isGrounded)
            {
                Vector2 offset = data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f);
                if (StateManager.CanStand(data, offset))
                {
                    // Execute animation transition
                    data.animator.Play(UnitAnimatorLayer.Body, "SlideExit");
                    // Update animator to transition to relevant state
                    data.animator.UpdateState();
                    transitionDuration = data.animator.GetState().length;
                    data.isStanding = true;
                    toIdle = true;
                }
            }
            
            if (data.isGrounded)
            {
                data.ApplyDrag(data.stats.slideDrag);
                // Execute Crawl
                if (data.rb.velocity.magnitude < data.stats.walkSpeed)
                {
                    data.animator.Play(UnitAnimatorLayer.Body, "Crawl_Idle");
                    return UnitState.Crawl;
                }
            }
            else
            {
                data.ApplyDrag(data.stats.airDrag);
            }
            
            StateManager.UpdateFacing(data);
            return UnitState.Slide;
        }
    }
}