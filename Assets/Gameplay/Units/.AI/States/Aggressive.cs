using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.States
{
    public class Aggressive : BaseState
    {
        public Aggressive(UnitData a_UnitData, AIData a_AIData) : base(a_UnitData, a_AIData) { }


        public override AIState Initialise()
        {
            return AIState.Aggressive;
        }

        public override AIState Execute()
        {

            return AIState.Aggressive;
        }

        public override void End()
        {
            
        }
        
    }
}
