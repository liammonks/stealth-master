using UnityEngine;

namespace States.StealthMaster
{
    public class Jump : States.Jump
    {
        public Jump(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            return base.Initialise();
        }

        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.Jump) return state;
            Vector2 velocity = unit.Physics.Velocity;

            // Check Vault (Require Momentum)
            if (Mathf.Abs(velocity.x) >= unit.Settings.runSpeed * 0.9f)
            {
                //UnitState vaultState = StateManager.TryVault(data);
                //if (vaultState != UnitState.Null)
                //{
                //    return vaultState;
                //}
            }
            // Wall Slide
            if (Mathf.Abs(velocity.x) > 0.1f && unit.StateMachine.FacingWall())
            {
                //return UnitState.WallSlide;
            }
            // Execute Dive
            if (unit.Input.Crawling && !unit.GroundSpring.enabled && unit.StateMachine.CanCrawl())
            {
                return UnitState.Dive;
            }
            // Melee
            if (unit.Input.Melee)
            {
                return UnitState.JumpMelee;
            }

            return UnitState.Jump;
        }


    }
}