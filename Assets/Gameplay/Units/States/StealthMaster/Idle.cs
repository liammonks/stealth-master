using UnityEngine;

namespace States.StealthMaster
{
    public class Idle : States.Idle
    {
        private bool toCrawl = false;
        private float transitionDuration = 0.0f;

        public Idle(UnitData a_data) : base(a_data) { }

        public override UnitState Initialise()
        {
            toCrawl = false;
            transitionDuration = 0.0f;
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            if (toCrawl)
            {
                // Waiting to enter crawl state
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                if (transitionDuration == 0.0f) return UnitState.CrawlIdle;
                return UnitState.Idle;
            }
            
            UnitState state = base.Execute();
            if (state != UnitState.Idle) return state;

            // Push against wall
            if (StateManager.FacingWall(data))
            {
                if (data.animator.CurrentState != "AgainstWall")
                {
                    data.rb.velocity = new Vector2(data.isFacingRight ? 0.5f : -0.5f, data.rb.velocity.y);
                    data.animator.Play(UnitAnimatorLayer.Body, "AgainstWall");
                }
            }
            else
            {
                data.animator.Play(UnitAnimatorLayer.Body, "Idle");
            }
            // Execute Melee
            if (data.input.meleeQueued)
            {
                return UnitState.Melee;
            }
            // Execute Crawl
            if (data.input.crawling && StateManager.CanCrawl(data))
            {
                // Play stand to crawl, wait before entering state
                data.animator.Play(UnitAnimatorLayer.Body, "StandToCrawl");
                data.animator.UpdateState();
                transitionDuration = data.animator.GetState().length;
                data.isStanding = false;
                toCrawl = true;
            }
            
            return UnitState.Idle;
        }
    }
}