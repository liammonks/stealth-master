using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.States
{    
    public abstract class BaseState
    {
        protected UnitData m_UnitData;
        protected UnitData m_AIData;

        public BaseState(UnitData a_UnitData, AIData a_AIData)
        {
            m_UnitData = a_UnitData;
            m_AIData = a_UnitData;
        }

        public abstract AIState Initialise();
        public abstract AIState Execute();
        public abstract void End();
    }
}