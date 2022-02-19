using UnityEngine;

namespace States
{
    public class Melee : BaseState
    {
        float duration = 0.0f;

        public Melee(UnitData a_data) : base(a_data) { }
        
        public override UnitState Initialise()
        {
            data.animator.Play(UnitAnimatorLayer.Body, "Melee");
            data.animator.UpdateState();
            duration = data.animator.GetState().length;
            data.rb.velocity = Vector2.zero;
            data.hitIDs.Clear();
            return UnitState.Melee;
        }
        
        public override UnitState Execute()
        {
            duration = Mathf.Max(0.0f, duration - Time.deltaTime);
            if (duration <= data.animator.GetState().length * 0.5f)
            {
                RaycastHit2D[] hits = Physics2D.BoxCastAll(
                    data.rb.position + data.stats.meleeOffset,
                    data.stats.meleeScale,
                    data.rb.rotation,
                    Vector2.zero,
                    0,
                    data.hitMask
                );
                ExtDebug.DrawBox(
                    data.rb.position + data.stats.meleeOffset,
                    data.stats.meleeScale * 0.5f,
                    Quaternion.Euler(0, 0, data.rb.rotation),
                    hits.Length > 0 ? Color.green : Color.red
                );
                foreach (RaycastHit2D hit in hits)
                {
                    Unit unit = hit.rigidbody?.GetComponent<Unit>();
                    if (unit && !data.hitIDs.Contains(unit.ID))
                    {
                        Vector2 impact = data.rb.velocity + (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.meleeKnockback * data.stats.knockbackMultiplier;
                        unit.TakeDamage(impact * data.stats.meleeDamage);
                        data.hitIDs.Add(unit.ID);
                    }
                }
            }
            if (duration == 0.0f)
            {
                return UnitState.Idle;
            }
            return UnitState.Melee;
        }
    }
}