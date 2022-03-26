using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarRating : MonoBehaviour
{
    [SerializeField] private GameObject starPrefab;
    private int rating = 0;


    private void Awake()
    {
        GlobalEvents.onMissionComplete += OnMissionComplete;
        GlobalEvents.onMissionRestart += OnMissionRestart;
        GlobalEvents.onEnemyKilled += OnEnemyKilled;
    }

    private void OnDestroy()
    {
        GlobalEvents.onMissionComplete -= OnMissionComplete;
        GlobalEvents.onMissionRestart -= OnMissionRestart;
        GlobalEvents.onEnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled()
    {
        rating++;
    }

    private void OnMissionRestart()
    {
        foreach(Transform child in transform)
        {
            Destroy(child);
        }
        rating = 0;
    }

    private void OnMissionComplete()
    {
        for(int i = 0; i < rating; ++i)
        {
            Instantiate(starPrefab, transform);
        }
    }
}
