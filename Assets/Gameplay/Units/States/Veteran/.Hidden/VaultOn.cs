using UnityEngine;

namespace States
{
    public class VaultOn : BaseState
    {
        protected float transitionDuration = 0.0f;

        public VaultOn(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            data.animator.Play(UnitAnimatorLayer.Body, "VaultOn");
            transitionDuration = data.stats.vaultDuration;
            data.isStanding = true;
            data.groundSpringActive = false;
            // Disable collider
            data.rb.bodyType = RigidbodyType2D.Kinematic;
            Debug.DrawRay(data.rb.position, data.target, Color.blue, data.t);
            return UnitState.VaultOn;
        }
        
        public override UnitState Execute()
        {
            if (transitionDuration > 0.0f)
            {
                transitionDuration = Mathf.Max(0.0f, transitionDuration - Time.fixedDeltaTime);
                data.rb.velocity = (data.target / data.stats.vaultDuration);
            }
            if (transitionDuration == 0.0f)
            {
                data.groundSpringActive = true;
                // Enable collider
                data.rb.bodyType = RigidbodyType2D.Dynamic;
                return UnitState.Run;
            }
            return UnitState.VaultOn;
        }
    }
}