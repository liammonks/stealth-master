using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitInput))]
public class Unit : MonoBehaviour
{
    public enum CollisionState
    {
        Standing,
        Crouching,
        Crawling
    }

    public UnitInput Input => m_Input;
    public UnitAnimator Animator => m_Animator;

    [SerializeField] private List<GameObject> m_GibPrefabs;

    private CollisionState m_CollisionState;
    private UnitInput m_Input;
    private UnitAnimator m_Animator;

    private void Awake() {
        m_Input = GetComponent<UnitInput>();
        m_Animator = GetComponent<UnitAnimator>();
    }

    private void Update()
    {
        
    }

    public void SpawnGibs(Vector2 position, float force)
    {
        foreach (GameObject gibPrefab in m_GibPrefabs)
        {
            GameObject gib = Instantiate(gibPrefab, position, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
            gib.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized * (force * Random.Range(0.5f, 1.0f));
        }
    }
}
