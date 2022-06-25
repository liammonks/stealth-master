using UnityEngine;

namespace States.StealthMaster
{
    public class Fall : States.Fall
    {
        public Fall(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.Fall) return state;

            // Check Ledge
            if (unit.StateMachine.TryLedgeGrab())
            {
                return UnitState.LedgeGrab;
            }
            // Wall Slide
            if (unit.StateMachine.FacingWall())
            {
                return UnitState.WallSlide;
            }
            // Execute Dive
            if (unit.Input.Crawling && unit.StateMachine.CanCrawl())
            {
                return UnitState.Dive;
            }
            
            return UnitState.Fall;
        }
    }
}