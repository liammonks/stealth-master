using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[RequireComponent(typeof(BehaviourTreeRunner))]
public class AIUnit : Unit
{
    public AIStats aiStats;
    
    private Vector2 healthBarOffset = new Vector2(0, 1);

    protected override void Start() {
        base.Start();
        // Init layer masks
        data.hitMask = LayerMask.GetMask("Player");
        healthBar = LevelManager.Instance.UI.HealthBarPool.Get();
    }

    private void Update() {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position + (Vector3)healthBarOffset);
        healthBar.GetComponent<RectTransform>().position = screenPosition;
    }

    public override void Die()
    {
        LevelManager.Instance.UI.HealthBarPool.Release(healthBar);
        Destroy(gameObject);
    }
}
