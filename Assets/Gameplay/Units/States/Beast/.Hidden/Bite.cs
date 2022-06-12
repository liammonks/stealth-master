using UnityEngine;

namespace States
{
    public class Bite : BaseState
    {
        public Bite(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.Bite;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.Bite;
        }
    }
}