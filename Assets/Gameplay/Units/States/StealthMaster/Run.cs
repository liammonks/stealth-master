using UnityEngine;

namespace States.StealthMaster
{
    public class Run : States.Run
    {
        protected bool toCrawl = false;
        protected float transitionDuration = 0.0f;

        public Run(UnitData a_data) : base(a_data) { }
        
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
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                if (transitionDuration == 0.0f) return UnitState.Crawl;
                return UnitState.Run;
            }
            
            UnitState state = base.Execute();
            if (state != UnitState.Run) return state;

            // Check Climb
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.walkSpeed * 0.75f)
            {
                UnitState climbState = StateManager.TryLedgeGrab(data);
                if (climbState != UnitState.Null)
                {
                    return climbState;
                }
            }
            // Check Vault
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.runSpeed * 0.75f)
            {
                UnitState vaultState = StateManager.TryVault(data);
                if (vaultState != UnitState.Null)
                {
                    return vaultState;
                }
            }
            // Face Wall
            if (StateManager.FacingWall(data))
            {
                data.animator.Play(UnitAnimatorLayer.Body, "AgainstWall");
            }
            else
            {
                data.animator.Play(UnitAnimatorLayer.Body, "Run");
            }
            // Execute Melee
            if (data.input.meleeQueued)
            {
                return UnitState.Melee;
            }

            // Crawl / Slide
            if (data.input.crawling && StateManager.CanCrawl(data))
            {
                if (Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed)
                {
                    // Execute Slide
                    data.animator.Play(UnitAnimatorLayer.Body, "Slide");
                    return UnitState.Slide;
                }
                else
                {
                    // Play stand to crawl, wait before entering state
                    data.animator.Play(UnitAnimatorLayer.Body, "StandToCrawl");
                    data.animator.UpdateState();
                    transitionDuration = data.animator.GetState().length;
                    data.isStanding = false;
                    toCrawl = true;
                }
            }

            return UnitState.Run;
        }
    }
}