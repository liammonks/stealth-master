using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States
{
    public abstract class BaseState
    {
        protected Unit owner;
        
        public BaseState(Unit a_owner)
        {
            owner = a_owner;
        }
        
        public abstract UnitState Initialise();
        public abstract UnitState Execute();
        public abstract UnitState Deinitialise();
    }
}