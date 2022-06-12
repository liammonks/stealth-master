using UnityEngine;

namespace States.StealthMaster
{
    public class Fall : States.Fall
    {
        public Fall(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.Fall) return state;

            // Check Climb
            UnitState climbState = StateManager.TryLedgeGrab(data);
            if (climbState != UnitState.Null)
            {
                return climbState;
            }
            // Wall Slide
            if (StateManager.FacingWall(data))
            {
                return UnitState.WallSlide;
            }
            // Execute Dive
            if (data.input.crawling && data.previousState != UnitState.WallSlide && StateManager.CanCrawl(data))
            {
                return UnitState.Dive;
            }
            
            return UnitState.Fall;
        }
    }
}