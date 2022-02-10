using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewReachTarget", menuName = "WinConditions/ReachTarget", order = 0)]
public class ReachTarget : WinCondition
{
    [SerializeField] private string targetID;

    protected override void OnEnable() {
        base.OnEnable();
        GlobalEvents.onTargetReached += OnTargetReached;
    }
    
    private void OnDisable() {
        GlobalEvents.onTargetReached -= OnTargetReached;
    }
    
    private void OnTargetReached(string a_targetID)
    {
        if (targetID != a_targetID) return;
        Complete();
    }
}
