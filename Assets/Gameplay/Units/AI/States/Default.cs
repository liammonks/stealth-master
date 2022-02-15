using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.States
{
    public class Default : BaseState
    {
        public Default(UnitData a_UnitData, AIData a_AIData) : base(a_UnitData, a_AIData) { }

        public override AIState Initialise()
        {
            return AIState.Default;
        }

        public override AIState Execute()
        {
            // Check hunger, interact with vending machines
            
            // Complete tasks
            
            // Move between patrol points
            
            return AIState.Default;
        }

        public override void End()
        {
            
        }

    }
}
