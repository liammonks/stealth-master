using UnityEngine;

namespace States
{
    public class Slide : BaseState
    {
        protected const float minSlideDuration = 0.5f;

        protected bool toIdle = false;
        protected float transitionDuration = 0.0f;
        protected float stateDuration = 0.0f;

        public Slide(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            toIdle = false;
            transitionDuration = 0.0f;
            stateDuration = 0.0f;
            unit.WallSpring.enabled = false;

            unit.SetBodyState(BodyState.Crawling, unit.Animator.CurrentStateLength);
            unit.Physics.SetDragState(UnitPhysics.DragState.Sliding);

            if (unit.StateMachine.PreviousState != UnitState.Dive)
            {
                unit.Physics.Velocity = unit.Physics.Velocity * unit.Settings.slideVelocityMultiplier;
            }

            return UnitState.Slide;
        }
        
        public override UnitState Execute()
        {
            stateDuration += DeltaTime;

            if (toIdle)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);

                // Execute Run / Idle
                if (transitionDuration == 0.0f)
                {
                    return Mathf.Abs(unit.Physics.Velocity.x) > unit.Settings.walkSpeed * 0.5f ? UnitState.Run : UnitState.Idle;
                }

                return UnitState.Slide;
            }
            
            // Transition Idle
            if (!unit.Input.Crawling && stateDuration > minSlideDuration && unit.GroundSpring.Intersecting && unit.StateMachine.CanStand())
            {
                // Execute animation transition
                unit.Animator.Play(unit.Animator.CurrentState == UnitAnimationState.BellySlide ? UnitAnimationState.CrawlToStand : UnitAnimationState.SlideExit);
                transitionDuration = unit.Animator.CurrentStateLength;
                unit.SetBodyState(BodyState.Standing, transitionDuration);
                toIdle = true;
            }
            
            if (unit.GroundSpring.Intersecting)
            {
                // Execute Crawl
                if (unit.Physics.Velocity.magnitude < unit.Settings.walkSpeed)
                {
                    unit.Animator.Play(UnitAnimationState.Crawl_Idle);
                    return UnitState.Crawl;
                }
            }
            
            return UnitState.Slide;
        }

        public override void Deinitialise()
        {
            unit.Physics.SetDragState(UnitPhysics.DragState.Default);
            unit.WallSpring.enabled = true;
        }
    }
}