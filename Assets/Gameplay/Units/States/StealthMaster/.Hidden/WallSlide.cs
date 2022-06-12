using UnityEngine;

namespace States.StealthMaster
{
    public class WallSlide : States.WallSlide
    {
        public WallSlide(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            UnitState state = base.Execute();
            if (state != UnitState.WallSlide) return state;
            
            // Execute Wall Jump
            if (data.rb.velocity.y > 0.0f)
            {
                // Jump Either Direction
                if (data.input.jumpQueued && StateManager.CanStand(data, new Vector2(data.isFacingRight ? -data.stats.standingHalfWidth : data.stats.standingHalfHeight, 0)))
                {
                    return UnitState.WallJump;
                }
                // Jump Away Right
                if (!data.isFacingRight && data.input.movement > 0 && StateManager.CanStand(data, new Vector2(data.stats.standingHalfWidth, 0)))
                {
                    return UnitState.WallJump;
                }
                // Jump Away Left
                if (data.isFacingRight && data.input.movement < 0 && StateManager.CanStand(data, new Vector2(-data.stats.standingHalfWidth, 0)))
                {
                    return UnitState.WallJump;
                }
            }
            
            return UnitState.WallSlide;
        }
    }
}