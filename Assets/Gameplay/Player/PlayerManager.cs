using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public Unit Unit => m_ActivePlayerObject.GetComponent<Unit>();

    [SerializeField] private PlayerCamera m_PlayerCamera;

    [SerializeField]
    private GameObject m_CleanPlayerObject;
    private GameObject m_ActivePlayerObject;

    private string m_PlayerName;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Debug.LogError("Two instances of PlayerManager found", this); }

        m_CleanPlayerObject.SetActive(false);
        m_PlayerName = m_CleanPlayerObject.name;
        m_CleanPlayerObject.name = m_PlayerName + " [CLEAN]";
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        SpawnPlayer(m_CleanPlayerObject.transform.position);
    }

    public void SpawnPlayer(Vector2 position)
    {
        if (m_ActivePlayerObject != null) { Destroy(m_ActivePlayerObject); }
        m_ActivePlayerObject = Instantiate(m_CleanPlayerObject, position, Quaternion.identity, transform);
        m_ActivePlayerObject.transform.SetParent(null);
        m_ActivePlayerObject.name = m_PlayerName + " [ACTIVE]";
        m_ActivePlayerObject.SetActive(true);
        m_PlayerCamera.SetTarget(m_ActivePlayerObject.transform);
    }
}
