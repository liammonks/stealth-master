using UnityEngine;

namespace States
{
    public class Alert : BaseState
    {
        public Alert(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.Alert;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.Alert;
        }
    }
}