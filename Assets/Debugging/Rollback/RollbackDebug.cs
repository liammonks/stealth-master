using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RollbackDebug : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Timeline;

    private void Awake()
    {
        m_Timeline.SetActive(false);
    }

    private void OnTimelineLeft()
    {
        EnableTimeline();
    }

    private void OnTimelineRight()
    {
        EnableTimeline();
    }

    private void OnTimelineReturn()
    {
        DisableTimeline();
    }

    private void EnableTimeline()
    {
        if (m_Timeline.activeInHierarchy) { return; }
        m_Timeline.SetActive(true);
        Time.timeScale = 0.0f;
    }

    private void DisableTimeline()
    {
        if (!m_Timeline.activeInHierarchy) { return; }
        m_Timeline.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
