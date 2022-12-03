using UnityEngine;

namespace States.StealthMaster
{
    public class WallJump : BaseState
    {
        protected float animationDuration;

        public WallJump(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            unit.GroundSpring.enabled = false;
            unit.WallSpring.enabled = false;
            unit.UpdateFacing = false;

            unit.FacingRight = !unit.FacingRight;
            DebugExtension.DebugArrow(unit.transform.position, unit.FacingRight ? Vector3.right : Vector3.left, Color.red, 3);
            unit.Physics.velocity = new Vector2((unit.FacingRight ? 1 : -1) * unit.Settings.wallJumpForce.x, unit.Settings.wallJumpForce.y);
            
            unit.Animator.Play(UnitAnimationState.WallJump);
            animationDuration = unit.Animator.CurrentStateLength;
            return UnitState.WallJump;
        }
        
        public override UnitState Execute()
        {
            // Execute Dive
            if (unit.Input.Crawling) { return UnitState.Dive; }

            // Continue WallJump
            animationDuration = Mathf.Max(0, animationDuration - DeltaTime);
            if (animationDuration != 0) { return UnitState.WallJump; }

            return UnitState.Fall;
        }

        public override void Deinitialise()
        {
            unit.UpdateFacing = true;
            unit.WallSpring.enabled = true;
        }
    }
}