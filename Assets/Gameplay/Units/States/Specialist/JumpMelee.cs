using UnityEngine;

namespace States
{
    public class JumpMelee : BaseState
    {
        public JumpMelee(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.JumpMelee;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.JumpMelee;
        }
    }
}