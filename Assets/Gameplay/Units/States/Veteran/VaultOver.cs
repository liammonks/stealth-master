using UnityEngine;

namespace States
{
    public class VaultOver : BaseState
    {
        protected float transitionDuration = 0.0f;

        public VaultOver(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            data.animator.Play("VaultOver");
            transitionDuration = data.stats.vaultDuration;
            data.isStanding = true;
            data.groundSpringActive = false;
            // Disable collider
            data.rb.bodyType = RigidbodyType2D.Kinematic;
            Debug.DrawRay(data.rb.position, data.target, Color.blue, data.t);
            return UnitState.VaultOver;
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
                return UnitState.Idle;
            }
            return UnitState.VaultOver;
        }
    }
}