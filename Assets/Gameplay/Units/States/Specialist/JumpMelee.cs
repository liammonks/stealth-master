using UnityEngine;

namespace States
{
    public class JumpMelee : BaseState
    {
        private float duration = 0.0f;

        public JumpMelee(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            data.groundSpringActive = true;
            data.animator.Play(UnitAnimatorLayer.Body, "JumpMelee");
            data.animator.UpdateState();
            duration = data.animator.GetState().length;
            data.hitIDs.Clear();
            return UnitState.JumpMelee;
        }
        
        public override UnitState Execute()
        {
            duration = Mathf.Max(0.0f, duration - Time.deltaTime);

            if (duration <= data.animator.GetState().length * 0.5f)
            {
                RaycastHit2D[] hits = Physics2D.BoxCastAll(
                    data.rb.position + (Vector2.up * data.stats.jumpMeleeOffset.y) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.jumpMeleeOffset.x),
                    data.stats.jumpMeleeScale,
                    data.rb.rotation,
                    Vector2.zero,
                    0,
                    data.hitMask
                );
                ExtDebug.DrawBox(
                    data.rb.position + (Vector2.up * data.stats.jumpMeleeOffset.y) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.jumpMeleeOffset.x),
                    data.stats.jumpMeleeScale * 0.5f,
                    Quaternion.Euler(0, 0, data.rb.rotation),
                    hits.Length > 0 ? Color.green : Color.red
                );
                foreach (RaycastHit2D hit in hits)
                {
                    Unit unit = hit.rigidbody?.GetComponent<Unit>();
                    if (unit && !data.hitIDs.Contains(unit.ID))
                    {
                        Vector2 impact = data.rb.velocity + (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.jumpMeleeKnockback * data.stats.knockbackMultiplier;
                        unit.TakeDamage(impact * data.stats.jumpMeleeDamage);
                        data.hitIDs.Add(unit.ID);
                    }
                }
            }
            if (duration == 0.0f)
            {
                return UnitState.Idle;
            }
            return UnitState.JumpMelee;
        }
    }
}