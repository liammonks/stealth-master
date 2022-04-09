using UnityEngine;

namespace States
{
    public class Pounce : BaseState
    {
        public Pounce(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.Pounce;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.Pounce;
        }
    }
}