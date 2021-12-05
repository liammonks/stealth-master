using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[RequireComponent(typeof(BehaviourTreeRunner))]
public class AIUnit : Unit
{
    [Header("AI")]
    public AIStats aiStats;
    
    private const float statsUpdateInterval = 1.0f;
    private Vector2 healthBarOffset = new Vector2(0, 1);

    protected override void Start() {
        base.Start();
        // Init stats
        aiStats = aiStats.CloneVariation(1.0f);
        StartCoroutine(UpdateStats());
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
    
    private IEnumerator UpdateStats()
    {
        yield return new WaitForSeconds(statsUpdateInterval);
        aiStats.thirst -= statsUpdateInterval;
        aiStats.hunger -= statsUpdateInterval;
        StartCoroutine(UpdateStats());
    }
}
