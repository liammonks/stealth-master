using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States
{
    public abstract class BaseState
    {
        protected UnitData data;
        
        public BaseState(UnitData a_data)
        {
            data = a_data;
        }
        
        public abstract UnitState Initialise();
        public abstract UnitState Execute();
    }
}