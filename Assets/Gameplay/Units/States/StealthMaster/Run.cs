using UnityEngine;

namespace States.StealthMaster
{
    public class Run : States.Run
    {
        protected bool toCrawl = false;
        protected float transitionDuration = 0.0f;

        public Run(Unit a_unit) : base(a_unit) { }
        
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
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);
                if (transitionDuration == 0.0f) return UnitState.Crawl;
                return UnitState.Run;
            }
            
            UnitState state = base.Execute();
            if (state != UnitState.Run) return state;
            Vector2 velocity = unit.Physics.Velocity;

            // Vault / Climb
            if (unit.StateMachine.TryVaultOver())
            {
                return UnitState.VaultOver;
            }
            if (unit.StateMachine.TryVaultOn())
            {
                return UnitState.VaultOn;
            }
            if (unit.StateMachine.CanClimb())
            {
                return UnitState.Climb;
            }

            // Execute Melee
            if (unit.Input.Melee)
            {
                return UnitState.Melee;
            }

            // Crawl / Slide
            if (unit.Input.Crawling && unit.StateMachine.CanCrawl())
            {
                if (Mathf.Abs(velocity.x) > unit.Settings.walkSpeed)
                {
                    // Execute Slide
                    unit.Animator.Play(UnitAnimationState.Slide);
                    return UnitState.Slide;
                }
                else
                {
                    // Play stand to crawl, wait before entering state
                    unit.Animator.Play(UnitAnimationState.StandToCrawl);
                    transitionDuration = unit.Animator.CurrentStateLength;
                    unit.SetBodyState(BodyState.Crawling, transitionDuration);
                    toCrawl = true;
                }
            }

            return UnitState.Run;
        }
    }
}