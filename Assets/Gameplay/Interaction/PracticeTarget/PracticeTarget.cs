using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeTarget : MonoBehaviour, ITakeDamage
{
    [SerializeField] private float maxHealth = 1.0f;
    private float health = 1;

    private void Awake()
    {
        health = maxHealth;
        GlobalEvents.onMissionRestart += OnMissionRestart;
    }

    private void OnDestroy()
    {
        GlobalEvents.onMissionRestart -= OnMissionRestart;
    }

    private void OnMissionRestart()
    {
        health = maxHealth;
        gameObject.SetActive(true);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            gameObject.SetActive(false);
            GlobalEvents.EnemyKilled();
        }
    }

    public void TakeDamage(Vector2 impact)
    {
        TakeDamage(impact.magnitude);
    }
}
