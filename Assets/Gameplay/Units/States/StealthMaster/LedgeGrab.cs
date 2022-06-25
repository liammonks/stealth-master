using UnityEngine;

namespace States.StealthMaster
{
    public class LedgeGrab : States.LedgeGrab
    {

        public LedgeGrab(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            if (!animationEnded) { return UnitState.LedgeGrab; }

            UnitState state = base.Execute();
            if (state != UnitState.LedgeGrab) return state;

            // Jump Up
            if (unit.Input.Jumping)
            {
                return UnitState.Jump;
            }
            // Jump Right
            if (!unit.FacingRight && unit.Input.Movement > 0)
            {
                return UnitState.WallJump;
            }
            // Jump Left
            if (unit.FacingRight && unit.Input.Movement < 0)
            {
                return UnitState.WallJump;
            }
            
            return UnitState.LedgeGrab;
        }
    }
}