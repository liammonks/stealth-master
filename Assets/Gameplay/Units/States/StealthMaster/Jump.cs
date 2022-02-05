using UnityEngine;

namespace States.StealthMaster
{
    public class Jump : States.Jump
    {
        public Jump(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.Jump) return state;

            // Check Vault (Require Momentum)
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.runSpeed * 0.9f)
            {
                UnitState vaultState = StateManager.TryVault(data);
                if (vaultState != UnitState.Null)
                {
                    return vaultState;
                }
            }
            // Wall Slide
            if (Mathf.Abs(data.rb.velocity.x) > 0.1f && StateManager.FacingWall(data))
            {
                return UnitState.WallSlide;
            }
            // Execute Dive
            if (!data.isGrounded || !data.groundSpringActive)
            {
                if (data.input.crawling && StateManager.CanCrawl(data))
                {
                    return UnitState.Dive;
                }
            }
            // Melee
            if (data.input.meleeQueued)
            {
                return UnitState.JumpMelee;
            }
            
            return UnitState.Jump;
        }
    }
}