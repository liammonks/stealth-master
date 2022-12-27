using UnityEngine;

namespace States.StealthMaster
{
    public class Slide : States.Slide
    {

        public Slide(Unit a_unit) : base(a_unit) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState baseReturnState = base.Execute();
            if (baseReturnState != UnitState.Slide) { return baseReturnState; }

            stateDuration += Time.fixedDeltaTime;

            // Grab on to ledges below
            if (stateDuration <= 0.3f && unit.StateMachine.PreviousState != UnitState.Dive)
            {
                if (unit.StateMachine.TryDrop())
                {
                    return UnitState.LedgeGrab;
                }
            }

            return UnitState.Slide;
        }
    }
}