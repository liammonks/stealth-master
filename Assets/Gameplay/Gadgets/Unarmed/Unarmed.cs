using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class Unarmed : BaseGadget
    {
        [SerializeField] private float power = 10.0f;
        [SerializeField] private Vector2 hitScale = new Vector2(0.5f, 0.5f);
        [SerializeField] private Vector2 hitOffset = new Vector2(0.5f, 0.4f);

        private const float spamDelay = 0.1f;

        protected override void OnPrimaryEnabled()
        {
            StartCoroutine(Punch());
        }

        protected override void OnPrimaryDisabled()
        {

        }

        private IEnumerator Punch()
        {
            primaryLocked = true;
            owner.SetState(UnitState.Null);
            owner.data.animator.Play(UnitAnimatorLayer.Body, "Punch");
            owner.data.animator.UpdateState();
            float duration = owner.data.animator.GetState().length;
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
                GameObject targetObject;
                if (hit.collider.attachedRigidbody != null) targetObject = hit.collider.attachedRigidbody.gameObject;
                else targetObject = hit.collider.gameObject;

                ITakeDamage target = targetObject.GetComponent(typeof(ITakeDamage)) as ITakeDamage;
                if (target != null)
                {
                    Vector2 impact = owner.data.rb.velocity + ((owner.data.isFacingRight ? Vector2.right : Vector2.left) * owner.data.stats.knockbackMultiplier);
                    target.TakeDamage(impact * power);
                }
            }

            yield return new WaitForSeconds(duration * 0.5f);
            owner.SetState(UnitState.Idle);
            yield return new WaitForSeconds(spamDelay);
            primaryLocked = false;
        }

        #region Block

        protected override void OnSecondaryEnabled()
        {
            owner.SetState(UnitState.Null);
            owner.data.animator.Play(UnitAnimatorLayer.Body, "Block");
            owner.data.rb.velocity = Vector2.zero;
            owner.onDamageTaken += OnDamageTaken;
        }
        
        private void OnDamageTaken()
        {
            owner.data.animator.Play(UnitAnimatorLayer.Body, "Block_Impact", true);
        }

        protected override void OnSecondaryDisabled()
        {
            owner.SetState(UnitState.Idle);
            owner.onDamageTaken -= OnDamageTaken;
        }

        #endregion
    }
}