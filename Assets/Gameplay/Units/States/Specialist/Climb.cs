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
            unit.UpdateFacingDirection = false;
            unit.WallSpring.enabled = false;
            unit.GroundSpring.enabled = false;
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
            unit.UpdateFacingDirection = true;
            unit.Physics.enabled = true;
            unit.GroundSpring.enabled = true;
            unit.WallSpring.enabled = true;
            unit.Animator.OnTranslationEnded -= OnClimbEnded;
        }

        private void OnClimbEnded()
        {
            climbEnded = true;
        }
    }
}