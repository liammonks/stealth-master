using UnityEngine;

namespace States
{
    public class Crouch : BaseState
    {
        protected bool toIdle = false;
        protected float transitionDuration = 0.0f;
        
        public Crouch(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            //data.animator.Play("Crouch");
            //data.isStanding = false;
            return UnitState.Crouch;
        }
        
        public override UnitState Execute()
        {
            // Execute Idle
            if (!data.input.crawling && data.isGrounded)
            {
                Vector2 standOffset = data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f);
                if (StateManager.CanStand(data, standOffset))
                {
                    // Play stand to crawl, wait before entering state
                    data.animator.Play(UnitAnimatorLayer.Body, "StandToCrawl");
                    data.animator.UpdateState();
                    transitionDuration = data.animator.GetState().length;
                    data.isStanding = false;
                    toIdle = true;
                }
            }
            return UnitState.Crouch;
        }
    }
}