using UnityEngine;

namespace States
{
    public class Launched : BaseState
    {
        public Launched(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.Launched;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.Launched;
        }
    }
}