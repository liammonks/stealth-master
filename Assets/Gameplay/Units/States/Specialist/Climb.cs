using UnityEngine;

namespace States
{
    public class Climb : BaseState
    {
        private bool climbEnded = false;

        public Climb(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            climbEnded = false;
            unit.Animator.OnTranslationEnded += OnClimbEnded;
            return UnitState.Climb;
        }

        public override UnitState Execute()
        {
            if (climbEnded)
            {
                return UnitState.Idle;
            }
            return UnitState.Climb;
        }

        public override void Deinitialise()
        {
            unit.Animator.OnTranslationEnded -= OnClimbEnded;
        }

        private void OnClimbEnded()
        {
            climbEnded = true;
        }
    }
}