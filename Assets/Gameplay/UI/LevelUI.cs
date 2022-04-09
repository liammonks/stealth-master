using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LevelUI : MonoBehaviour
{
    public IObjectPool<HealthBar> HealthBarPool => m_HealthBarPool;
    private IObjectPool<HealthBar> m_HealthBarPool;

    [SerializeField] private HealthBar healthBarPrefab;

    public bool collectionChecks = true;
    private const int healthbarCount = 5;
    
    public void Awake() {
        m_HealthBarPool = new ObjectPool<HealthBar>(CreateHealthBar, GetHealthBar, ReleaseHealthBar, DestroyHealthBar, collectionChecks, healthbarCount);
    }

    private HealthBar CreateHealthBar()
    {
        return Instantiate(healthBarPrefab, transform);
    }

    // Called when an item is taken from the pool using Get
    void GetHealthBar(HealthBar healthBar)
    {
        healthBar.gameObject.SetActive(true);
    }

    // Called when an item is returned to the pool using Release
    void ReleaseHealthBar(HealthBar healthBar)
    {
        healthBar.gameObject.SetActive(false);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void DestroyHealthBar(HealthBar healthBar)
    {
        Destroy(healthBar.gameObject);
    }
}
