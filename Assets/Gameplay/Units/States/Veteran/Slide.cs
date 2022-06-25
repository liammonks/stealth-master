using UnityEngine;

namespace States
{
    public class Slide : BaseState
    {
        protected bool toIdle = false;
        protected float transitionDuration = 0.0f;

        public Slide(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            transitionDuration = 0.0f;
            unit.SetBodyState(BodyState.Crawling, unit.Animator.CurrentStateLength);
            if (unit.StateMachine.PreviousState != UnitState.Dive)
            {
                unit.Physics.SetVelocity(unit.Physics.Velocity * unit.Settings.slideVelocityMultiplier);
            }
            return UnitState.Slide;
        }
        
        public override UnitState Execute()
        {
            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);
                unit.Physics.ApplyDrag(unit.Settings.slideDrag);
                // Execute Run / Idle
                if (transitionDuration == 0.0f) return Mathf.Abs(unit.Physics.Velocity.x) > unit.Settings.walkSpeed * 0.5f ? UnitState.Run : UnitState.Idle;
                return UnitState.Slide;
            }
            
            // Transition Idle
            if (!unit.Input.Crawling && unit.GroundSpring.Grounded)
            {
                if (unit.StateMachine.CanStand())
                {
                    // Execute animation transition
                    unit.Animator.Play(unit.Animator.CurrentState == UnitAnimationState.BellySlide ? UnitAnimationState.CrawlToStand : UnitAnimationState.SlideExit);
                    transitionDuration = unit.Animator.CurrentStateLength;
                    unit.SetBodyState(BodyState.Standing, transitionDuration);
                    toIdle = true;
                }
            }
            
            if (unit.GroundSpring.Grounded)
            {
                unit.Physics.ApplyDrag(unit.Settings.slideDrag);
                // Execute Crawl
                if (unit.Physics.Velocity.magnitude < unit.Settings.walkSpeed)
                {
                    unit.Animator.Play(UnitAnimationState.Crawl_Idle);
                    return UnitState.Crawl;
                }
            }
            else
            {
                unit.Physics.ApplyDrag(unit.Settings.airDrag);
            }
            
            return UnitState.Slide;
        }

        public override void Deinitialise()
        {
            unit.Input.Crawling = false;
        }
    }
}