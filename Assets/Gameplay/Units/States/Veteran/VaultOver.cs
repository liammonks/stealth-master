using UnityEngine;

namespace States
{
    public class VaultOver : BaseState
    {
        private bool vaultEnded = false;

        public VaultOver(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            vaultEnded = false;
            unit.Animator.OnTranslationEnded += OnVaultEnded;
            return UnitState.VaultOver;
        }
        
        public override UnitState Execute()
        {
            if (vaultEnded)
            {
                return UnitState.Idle;
            }
            return UnitState.VaultOver;
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