using DarkRift;
using System.Collections.Generic;
using UnityEngine;

namespace States.StealthMaster
{
    public class Jump : States.Jump, IRollback
    {

        public Jump(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            return base.Initialise();
        }

        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.Jump) return state;
            Vector2 velocity = unit.Physics.Velocity;

            // Wall Slide
            if (unit.WallSpring.Intersecting)
            {
                return UnitState.WallSlide;
            }
            // Execute Dive
            if (unit.Input.Crawling && !unit.GroundSpring.enabled && unit.StateMachine.CanCrawl())
            {
                return UnitState.Dive;
            }
            // Melee
            if (unit.Input.Melee)
            {
                return UnitState.JumpMelee;
            }

            return UnitState.Jump;
        }

    }
}