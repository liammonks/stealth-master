using UnityEngine;

namespace States
{
    public class Climb : BaseState
    {
        protected float climbDuration = 0.0f;

        public Climb(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            climbDuration = 0.0f;
            data.animator.Play("Climb");
            climbDuration = data.stats.climbDuration;
            data.target = data.rb.position + (Vector2.up * (data.stats.standingHalfHeight - data.stats.climbGrabOffset.y)) + ((data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x);
            data.target = (data.target - data.rb.position) / climbDuration;
            data.rb.isKinematic = true;
            return UnitState.Climb;
        }
        
        public override UnitState Execute()
        {
            climbDuration = Mathf.Max(0, climbDuration - Time.fixedDeltaTime);
            
            if (climbDuration == 0.0f)
            {
                data.rb.velocity = Vector2.zero;
                data.rb.isKinematic = false;
                data.groundSpringActive = true;
                return UnitState.Idle;
            }
            else
            {
                data.rb.velocity = data.target;
                Debug.DrawRay(
                    data.rb.position,
                    data.target,
                    Color.blue,
                    Time.fixedDeltaTime
                );
            }

            return UnitState.Climb;
        }
    }
}