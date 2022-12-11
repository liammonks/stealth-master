using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public Action<Unit> OnUnitSpawned;

    public Unit Unit => m_PlayerUnit;
    private Unit m_PlayerUnit;

    [SerializeField]
    private PlayerCamera m_PlayerCamera;

    [SerializeField]
    private Transform m_SpawnPoint;

    [SerializeField]
    private Unit m_PlayerUnitPrefab;


    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Debug.LogError("Two instances of PlayerManager found", this); }

        SpawnPlayer();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Select the active player object if the clean one was selected
    /// </summary>
    private void Start()
    {
        if (Selection.activeGameObject == m_SpawnPoint)
        {
            Selection.activeGameObject = m_PlayerUnit.gameObject;
        }
    }
#endif

    public void SpawnPlayer()
    {
        SpawnPlayer(m_SpawnPoint.position);
    }

    public void SpawnPlayer(Vector2 position)
    {
        if (m_PlayerUnit != null) { Destroy(m_PlayerUnit.gameObject); }
        m_PlayerUnit = Instantiate(m_PlayerUnitPrefab, position, Quaternion.identity, transform);
        m_PlayerUnit.transform.SetParent(null);
        m_PlayerCamera.SetTarget(m_PlayerUnit.GetComponentInChildren<UnitAnimator>().transform);
        OnUnitSpawned?.Invoke(m_PlayerUnit);
    }

}
