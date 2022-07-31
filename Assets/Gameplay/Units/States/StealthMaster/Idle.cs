using UnityEngine;

namespace States.StealthMaster
{
    public class Idle : States.Idle
    {
        private bool toCrawl = false;
        private float transitionDuration = 0.0f;

        public Idle(Unit a_unit) : base(a_unit) { }

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
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);
                if (transitionDuration == 0.0f) return UnitState.CrawlIdle;
                return UnitState.Idle;
            }

            UnitState state = base.Execute();
            if (state != UnitState.Idle) return state;

            // Push against wall
            if (unit.WallSpring.Intersecting)
            {
                if (unit.Animator.CurrentState != UnitAnimationState.AgainstWall)
                {
                    unit.Physics.SetVelocity(new Vector2(unit.FacingRight ? 0.5f : -0.5f, unit.Physics.Velocity.y));
                    unit.Animator.Play(UnitAnimationState.AgainstWall);
                }
            }
            else
            {
                unit.Animator.Play(UnitAnimationState.Idle);
            }
            // Execute Melee
            if (unit.Input.Melee)
            {
                return UnitState.Melee;
            }
            // Execute Crawl
            if (ToCrawl()) { return UnitState.CrawlIdle; }

            return UnitState.Idle;
        }

        private bool ToCrawl()
        {
            if (!unit.Input.Crawling) { return false; }
            if (unit.StateMachine.GetLastExecutionTime(UnitState.LedgeGrab) < 0.2f) { return false; }
            if (!unit.StateMachine.CanCrawl()) { return false; }

            // Play stand to crawl, wait before entering state
            unit.Animator.Play(UnitAnimationState.StandToCrawl);
            transitionDuration = unit.Animator.CurrentStateLength;
            unit.SetBodyState(BodyState.Crawling, transitionDuration);
            toCrawl = true;
            return true;
        }
    }

}