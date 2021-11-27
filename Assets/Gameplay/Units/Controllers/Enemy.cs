using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    private Vector2 healthBarOffset = new Vector2(0, 1);

    protected override void Start() {
        base.Start();
        // Init layer masks
        data.hitMask = LayerMask.GetMask("Player");
        healthBar = LevelManager.Instance.UI.healthBarPool.Get();
    }

    private void Update() {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position + (Vector3)healthBarOffset);
        healthBar.GetComponent<RectTransform>().position = screenPosition;
    }

    public override void Die()
    {
        LevelManager.Instance.UI.healthBarPool.Release(healthBar);
        Destroy(gameObject);
    }
}
