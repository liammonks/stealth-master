using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States
{
    public abstract class BaseState
    {

        protected Unit unit;
        
        public BaseState(Unit a_unit)
        {
            unit = a_unit;
        }
        
        public abstract UnitState Initialise();
        public abstract UnitState Execute();
        public abstract void Deinitialise();

    }
}