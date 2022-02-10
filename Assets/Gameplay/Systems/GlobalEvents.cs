using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalEvents
{
    public delegate void OnMissionComplete();
    public static event OnMissionComplete onMissionComplete;
    public static void MissionComplete() => onMissionComplete?.Invoke();

    public delegate void OnMissionRestart();
    public static event OnMissionRestart onMissionRestart;
    public static void MissionRestart() => onMissionRestart?.Invoke();
    
    public delegate void OnEnemyKilled();
    public static event OnEnemyKilled onEnemyKilled;
    public static void EnemyKilled() => onEnemyKilled?.Invoke();

    public delegate void OnTargetReached(string targetID);
    public static event OnTargetReached onTargetReached;
    public static void TargetReached(string targetID) => onTargetReached?.Invoke(targetID);
}
