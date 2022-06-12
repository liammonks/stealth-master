using UnityEngine;

namespace States.StealthMaster
{
    public class Dive : BaseState
    {
        protected bool toStand = false;
        protected float groundSpringPrevention = 0.0f;
        protected float transitionDuration = 0.0f;

        public Dive(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            toStand = false;
            unit.Animator.Play(UnitAnimationState.Dive);
            unit.SetBodyState(BodyState.Crawling, unit.Animator.CurrentStateLength);
            // Boost when diving from jump
            if (unit.StateMachine.PreviousState == UnitState.Jump || unit.StateMachine.PreviousState == UnitState.WallJump)
            {
                //data.rb.velocity *= data.stats.diveVelocityMultiplier;
                Vector2 velocity = unit.Physics.Velocity;
                velocity.x += 5 * Mathf.Sign(velocity.x);
                velocity.y += 2 * Mathf.Sign(velocity.y);
                unit.Physics.SetVelocity(velocity);
            }
            // Set timer to stop ground spring
            unit.GroundSpring.enabled = false;
            groundSpringPrevention = 0.2f;
            return UnitState.Dive;
        }
        
        public override UnitState Execute()
        {
            if (toStand)
            {
                unit.Physics.ApplyDrag(unit.Settings.slideDrag);
                transitionDuration = Mathf.Max(0.0f, transitionDuration - DeltaTime);
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
                velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * DeltaTime;
                unit.Physics.SetVelocity(velocity);
            }
            else
            {
                unit.Physics.ApplyDrag(unit.Settings.airDrag);
            }
            
            // Re-enable ground spring after delay
            if (!unit.GroundSpring.enabled)
            {
                groundSpringPrevention = Mathf.Max(0.0f, groundSpringPrevention - DeltaTime);
                if (groundSpringPrevention == 0.0f)
                {
                    unit.GroundSpring.enabled = true;
                }
            }
            // Check Landed
            else if (unit.GroundSpring.Grounded)
            {
                // Not crawling, stand up
                if (!unit.Input.Crawling)
                {
                    if (unit.StateMachine.CanStand())
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

        }
    }
}