using UnityEngine;

namespace States
{
    public class VaultOn : BaseState
    {
        private bool vaultEnded = false;

        public VaultOn(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            vaultEnded = false;
            unit.Animator.OnTranslationEnded += OnVaultEnded;
            unit.WallSpring.enabled = false;
            return UnitState.VaultOn;
        }
        
        public override UnitState Execute()
        {
            if (vaultEnded)
            {
                return UnitState.Idle;
            }
            return UnitState.VaultOn;
        }

        public override void Deinitialise()
        {
            unit.Animator.OnTranslationEnded -= OnVaultEnded;
            unit.WallSpring.enabled = true;
            unit.Physics.velocity = Vector2.zero;
        }

        private void OnVaultEnded()
        {
            vaultEnded = true;
        }

    }
}