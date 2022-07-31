using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States
{
    public abstract class BaseState
    {
        public float DeltaTime => tickRate == 0 ? Time.deltaTime : tickRate;

        protected Unit unit;

        private float tickRate;
        
        public BaseState(Unit a_unit)
        {
            unit = a_unit;
            tickRate = unit.StateMachine.TickRate;
        }
        
        public abstract UnitState Initialise();
        public abstract UnitState Execute();
        public abstract void Deinitialise();
    }
}