using UnityEngine;

namespace States
{
    public class LedgeGrab : BaseState
    {
        protected bool againstWall = false;
        protected bool animationEnded = false;

        public LedgeGrab(Unit a_unit) : base(a_unit) 
        {
            updateFacing = false;
        }
        
        public override UnitState Initialise()
        {
            animationEnded = false;
            unit.Physics.enabled = false;
            unit.GroundSpring.enabled = false;
            unit.Physics.SetVelocity(Vector2.zero);
            if (unit.StateMachine.PreviousState == UnitState.Slide) { unit.FacingRight = !unit.FacingRight; }
            unit.Animator.OnAnimationEnded += OnAnimationEnded;
            return UnitState.LedgeGrab;
        }
        
        public override UnitState Execute()
        {
            if (!animationEnded) { return UnitState.LedgeGrab; }

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
                unit.Input.Crawling = false;
                return againstWall ? UnitState.WallSlide : UnitState.Fall;
            }

            return UnitState.LedgeGrab;
        }

        public override void Deinitialise()
        {

        }

        private void OnAnimationEnded(UnitAnimationState state)
        {
            const float wallCheckBuffer = 0.01f;
            unit.Animator.OnAnimationEnded -= OnAnimationEnded;

            // Check if the ledge wall extends down to feet
            RaycastHit2D feetHit = Physics2D.Raycast(
                unit.Physics.WorldCenterOfMass + (Vector2.down * unit.Collider.Info[BodyState.Standing].Height * 0.5f),
                unit.FacingRight ? Vector2.right : Vector2.left,
                unit.Settings.climbGrabOffset.x + wallCheckBuffer,
                8
            );
            Debug.DrawRay(
                unit.Physics.WorldCenterOfMass + (Vector2.down * unit.Collider.Info[BodyState.Standing].Height * 0.5f),
                (unit.FacingRight ? Vector2.right : Vector2.left) * (unit.Settings.climbGrabOffset.x + wallCheckBuffer),
                feetHit ? Color.green : Color.red
            );
            againstWall = feetHit;
            unit.Animator.Play(againstWall ? UnitAnimationState.LedgeGrab : UnitAnimationState.LedgeGrab_Hang);

            animationEnded = true;
        }
    }
}