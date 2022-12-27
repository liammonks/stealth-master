using UnityEngine;

namespace States.StealthMaster
{
    public class Fall : States.Fall
    {
        protected float stateDuration = 0.0f;

        public Fall(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            stateDuration = 0.0f;
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            stateDuration += Time.fixedDeltaTime;
            UnitState state = base.Execute();
            if (state != UnitState.Fall) return state;

            // Check Ledge - Wait if we just came from ledge grab
            if (unit.StateMachine.PreviousState != UnitState.LedgeGrab || stateDuration >= 0.2f)
            {
                if (unit.StateMachine.TryLedgeGrab())
                {
                    return UnitState.LedgeGrab;
                }
            }
            // Wall Slide
            if (unit.WallSpring.Intersecting)
            {
                return UnitState.WallSlide;
            }

            if (ToDive()) { return UnitState.Dive; }
            
            return UnitState.Fall;
        }

        private bool ToDive()
        {
            if (!unit.Input.Crawling) { return false; }
            if (unit.StateMachine.GetLastExecutionTime(UnitState.LedgeGrab) < 0.2f) { return false; }
            return unit.StateMachine.CanCrawl();
        }
    }
}