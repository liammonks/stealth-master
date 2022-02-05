using UnityEngine;

namespace States
{
    public class Melee : BaseState
    {
        public Melee(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.Melee;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.Melee;
        }
    }
}