using UnityEngine;

namespace States
{
    public class HitImpact : BaseState
    {
        public HitImpact(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            
            return UnitState.HitImpact;
        }
        
        public override UnitState Execute()
        {
            
            return UnitState.HitImpact;
        }
    }
}