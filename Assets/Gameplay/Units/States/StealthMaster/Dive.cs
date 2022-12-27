using UnityEngine;

namespace States.StealthMaster
{
    public class Dive : BaseState
    {
        protected const float groundSpringActivationTime = 0.5f;

        protected bool toStand = false;
        protected float stateDuration = 0.0f;
        protected float transitionDuration = 0.0f;

        public Dive(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            toStand = false;
            unit.WallSpring.enabled = false;
            unit.Animator.Play(UnitAnimationState.Dive);
            unit.SetBodyState(BodyState.Crawling, unit.Animator.CurrentStateLength);
            // Boost when diving from jump
            if (unit.StateMachine.PreviousState == UnitState.Jump || unit.StateMachine.PreviousState == UnitState.WallJump)
            {
                //data.rb.velocity *= data.stats.diveVelocityMultiplier;
                Vector2 velocity = unit.Physics.Velocity;
                velocity.x += 5 * Mathf.Sign(velocity.x);
                velocity.y += 2 * Mathf.Sign(velocity.y);
                unit.Physics.Velocity = velocity;
            }
            // Set timer to stop ground spring
            unit.GroundSpring.enabled = false;
            stateDuration = 0.0f;
            return UnitState.Dive;
        }
        
        public override UnitState Execute()
        {
            // Transition to Idle
            if (toStand)
            {
                // Slide drag while transitioning
                unit.Physics.SetDragState(UnitPhysics.DragState.Sliding);
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);

                // Execute Idle
                if (transitionDuration == 0.0f)
                {
                    return UnitState.Idle;
                }

                return UnitState.Dive;
            }
            
            // Allow player to push towards movement speed while in the air
            Vector2 velocity = unit.Physics.Velocity;
            if (unit.Input.Movement != 0 && !unit.GroundSpring.Slipping && Mathf.Abs(velocity.x) < unit.Settings.walkSpeed)
            {
                float desiredSpeed = unit.Settings.walkSpeed * unit.Input.Movement;
                float deltaSpeedRequired = desiredSpeed - velocity.x;
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * Time.fixedDeltaTime;
                unit.Physics.Velocity = velocity;
            }
            
            // Re-enable ground spring after delay
            if (!unit.GroundSpring.enabled)
            {
                stateDuration += Time.fixedDeltaTime;
                if (stateDuration >= groundSpringActivationTime || !unit.Input.Crawling)
                {
                    unit.GroundSpring.enabled = true;
                }
            }
            // Check Landed
            else if (unit.GroundSpring.Intersecting)
            {
                // Not crawling, stand up
                if (!unit.Input.Crawling && unit.StateMachine.CanStand())
                {
                    if (Mathf.Abs(velocity.x) > unit.Settings.runSpeed)
                    {
                        unit.Animator.Play(UnitAnimationState.DiveFlip);
                        transitionDuration = unit.Animator.CurrentStateLength;                            
                        unit.SetBodyState(BodyState.Standing, transitionDuration * 0.5f);
                    }
                    else
                    {
                        unit.Animator.Play(UnitAnimationState.CrawlToStand);
                        transitionDuration = unit.Animator.CurrentStateLength;
                        unit.SetBodyState(BodyState.Standing, transitionDuration);
                    }
                    //data.LockGadget();
                    toStand = true;
                }
                else
                {
                    // Landed but still crawling
                    if (Mathf.Abs(velocity.x) > unit.Settings.walkSpeed)
                    {
                        // Execute Slide
                        unit.Animator.Play(UnitAnimationState.BellySlide);
                        return UnitState.Slide;
                    }
                    else
                    {
                        // Execute Crawl
                        unit.Animator.Play(UnitAnimationState.Crawl_Idle);
                        return UnitState.Crawl;
                    }
                }
            }
            
            return UnitState.Dive;
        }

        public override void Deinitialise()
        {
            unit.Physics.SetDragState(UnitPhysics.DragState.Default);
            unit.WallSpring.enabled = true;
        }
    }
}