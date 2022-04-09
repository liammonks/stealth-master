using UnityEngine;
using TMPro;
using System;

public class MissionTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerUI;

    [HideInInspector] public bool active = true;
    private float missionTimer = 0.0f;

    private void OnEnable()
    {
        GlobalEvents.onMissionComplete += MissionComplete;
        GlobalEvents.onMissionRestart += MissionRestart;
    }
    private void OnDisable()
    {
        GlobalEvents.onMissionComplete -= MissionComplete;
        GlobalEvents.onMissionRestart -= MissionRestart;
    }

    private void MissionComplete() => active = false;
    
    private void MissionRestart()
    {
        missionTimer = 0.0f;
        active = true;
    }

    private void Update()
    {
        if (!active) return;
        missionTimer += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(missionTimer);
        timerUI.text = time.TotalMinutes > 60 ? "idiot." : time.ToString("mm':'ss':'ff");
    }
}
