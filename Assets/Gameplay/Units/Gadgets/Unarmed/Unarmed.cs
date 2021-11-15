using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unarmed : Gadget
{
    [SerializeField] private float damage = 10.0f;
    [SerializeField] private float knockback = 1.0f;
    [SerializeField] private Vector2 hitScale = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 hitOffset = new Vector2(0.5f, 0.4f);

    protected override void OnPrimaryEnabled()
    {
        StartCoroutine(Punch());
    }

    protected override void OnPrimaryDisabled()
    {

    }

    private IEnumerator Punch()
    {
        owner.SetState(UnitState.Null);
        owner.data.animator.Play("Punch");
        owner.data.animator.Update(0);
        owner.data.animator.Update(0);
        float duration = owner.data.animator.GetCurrentAnimatorStateInfo(0).length;
        owner.data.rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration * 0.5f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
                owner.data.rb.position + (Vector2.up * hitOffset.y) + ((owner.data.isFacingRight ? Vector2.right : Vector2.left) * hitOffset.x),
                hitScale,
                owner.data.rb.rotation,
                Vector2.zero,
                0,
                owner.data.hitMask
            );
        ExtDebug.DrawBox(
            owner.data.rb.position + (Vector2.up * hitOffset.y) + ((owner.data.isFacingRight ? Vector2.right : Vector2.left) * hitOffset.x),
            hitScale * 0.5f,
            Quaternion.Euler(0, 0, owner.data.rb.rotation),
            hits.Length > 0 ? Color.green : Color.red,
            duration * 0.5f
        );
        foreach (RaycastHit2D hit in hits)
        {
            Unit unit = hit.rigidbody?.GetComponent<Unit>();
            if (unit)
            {
                unit.TakeDamage(
                    damage,
                    owner.data.rb.velocity + ((owner.data.isFacingRight ? Vector2.right : Vector2.left) * knockback * owner.data.stats.knockbackMultiplier)
                );
            }
        }

        yield return new WaitForSeconds(duration * 0.5f);
        owner.SetState(UnitState.Idle);
    }

    protected override void OnSecondaryEnabled()
    {
        owner.SetState(UnitState.Null);
        owner.data.animator.Play("Block");
        owner.data.rb.velocity = Vector2.zero;
    }

    protected override void OnSecondaryDisabled()
    {
        owner.SetState(UnitState.Idle);
    }

}
