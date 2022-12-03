using UnityEngine;

namespace States
{
    public class LedgeGrab : BaseState
    {
        protected const float inputLockDuration = 0.2f;

        protected bool againstWall = false;
        protected bool animationEnded = false;
        protected float stateDuration = 0.0f;

        public LedgeGrab(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            animationEnded = false;
            stateDuration = 0.0f;
            unit.UpdateFacing = false;
            unit.WallSpring.enabled = false;
            unit.GroundSpring.enabled = false;

            unit.Physics.velocity = Vector2.zero;
            if (unit.StateMachine.PreviousState == UnitState.Slide)
            {
                unit.FacingRight = !unit.FacingRight;
                unit.Animator.Play(UnitAnimationState.SlideToHang);
                unit.Animator.OnTranslationEnded += OnTranslationEnded;
            }
            else
            {
                //unit.Animator.Play()
                OnTranslationEnded();
            }
            return UnitState.LedgeGrab;
        }
        
        public override UnitState Execute()
        {
            stateDuration += DeltaTime;
            if (!animationEnded || stateDuration < inputLockDuration) { return UnitState.LedgeGrab; }

            if (unit.FacingRight)
            {
                // Climb Right
                if (unit.Input.Movement > 0 && unit.StateMachine.TryClimb())
                {
                    return UnitState.Climb;
                }
            }
            else
            {
                // Climb Left
                if (unit.Input.Movement < 0 && unit.StateMachine.TryClimb())
                {
                    return UnitState.Climb;
                }
            }

            // Drop
            if (unit.Input.Crawling)
            {
                return againstWall ? UnitState.WallSlide : UnitState.Fall;
            }

            return UnitState.LedgeGrab;
        }

        public override void Deinitialise()
        {
            unit.UpdateFacing = true;
            unit.WallSpring.enabled = true;
            unit.GroundSpring.enabled = true;
            unit.Physics.simulated = true;
        }

        private void OnTranslationEnded()
        {
            const float wallCheckBuffer = 0.01f;
            unit.Animator.OnTranslationEnded -= OnTranslationEnded;

            // Check if the ledge wall extends down to feet
            RaycastHit2D feetHit = Physics2D.Raycast(
                unit.Physics.worldCenterOfMass + (Vector2.down * unit.Collider.Info[BodyState.Standing].Height * 0.5f),
                unit.FacingRight ? Vector2.right : Vector2.left,
                unit.Settings.climbGrabOffset.x + wallCheckBuffer,
                8
            );
            Debug.DrawRay(
                unit.Physics.worldCenterOfMass + (Vector2.down * unit.Collider.Info[BodyState.Standing].Height * 0.5f),
                (unit.FacingRight ? Vector2.right : Vector2.left) * (unit.Settings.climbGrabOffset.x + wallCheckBuffer),
                feetHit ? Color.green : Color.red
            );
            againstWall = feetHit;
            unit.Animator.Play(againstWall ? UnitAnimationState.LedgeGrab : UnitAnimationState.LedgeGrab_Hang);
            unit.Physics.simulated = false;
            animationEnded = true;
        }
    }
}