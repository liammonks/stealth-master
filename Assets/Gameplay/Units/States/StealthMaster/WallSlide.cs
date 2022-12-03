using UnityEngine;

namespace States.StealthMaster
{
    public class WallSlide : States.WallSlide
    {
        public WallSlide(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.WallSlide) return state;
            
            // Execute Wall Jump
            if (unit.Physics.velocity.y > 0.0f)
            {
                // Jump Either Direction
                if (unit.Input.Jumping)
                {
                    return UnitState.WallJump;
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
            }
            
            return UnitState.WallSlide;
        }
    }
}