using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollbackDebug : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Timeline;

    private void Awake()
    {
        m_Timeline.SetActive(false);
    }

    private void Update()
    {
        if (m_Timeline.activeInHierarchy && Input.GetKeyDown(KeyCode.Return))
        {
            m_Timeline.SetActive(false);
            Time.timeScale = 1.0f;
        }
        if (!m_Timeline.activeInHierarchy && (Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.RightBracket)))
        {
            m_Timeline.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

}
