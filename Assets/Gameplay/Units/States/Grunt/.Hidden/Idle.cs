using UnityEngine;

namespace States.Grunt
{
    public class Idle : States.Idle
    {
        protected bool toCrouch = false;
        protected float transitionDuration = 0.0f;
        
        public Idle(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            //if (toCrouch)
            //{
            //    // Waiting to enter crouch state
            //    transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
            //    if (transitionDuration == 0.0f) return UnitState.Crouch;
            //    return UnitState.Idle;
            //}
            
            UnitState state = base.Execute();
            if (state != UnitState.Idle) return state;

            // Execute Crouch
            //if (data.input.crawling && StateManager.CanCrouch(data))
            //{
            //    // Play stand to crawl, wait before entering state
            //    data.animator.Play("StandToCrawl");
            //    data.animator.UpdateState();
            //    transitionDuration = data.animator.GetState().length;
            //    data.isStanding = false;
            //    toCrouch = true;
            //}
            return UnitState.Idle;
        }
    }
}