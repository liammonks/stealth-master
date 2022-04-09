using UnityEngine;

namespace States.StealthMaster
{
    public class LedgeGrab : States.LedgeGrab
    {
        public LedgeGrab(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.LedgeGrab) return state;

            if (data.stateDuration >= inputLockDuration)
            {
                // Jump Up
                if (data.input.jumpQueued)
                {
                    return UnitState.Jump;
                }
                // Jump Right
                if (!data.isFacingRight && data.input.movement > 0 && StateManager.CanStand(data, new Vector2(data.stats.standingHalfWidth, 0)))
                {
                    return UnitState.WallJump;
                }
                // Jump Left
                if (data.isFacingRight && data.input.movement < 0 && StateManager.CanStand(data, new Vector2(-data.stats.standingHalfWidth, 0)))
                {
                    return UnitState.WallJump;
                }
            }
            
            return UnitState.LedgeGrab;
        }
    }
}