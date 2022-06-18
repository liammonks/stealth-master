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
        }

        private void OnVaultEnded()
        {
            vaultEnded = true;
        }

    }
}