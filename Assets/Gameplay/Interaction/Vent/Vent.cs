using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vent : Interactable
{
    [Header("Connected Vents")]
    [SerializeField] private Vent upVent;
    [SerializeField] private Vent downVent;
    [SerializeField] private Vent leftVent;
    [SerializeField] private Vent rightVent;

    private List<Unit> activeUnits = new List<Unit>();

    public override bool Interact(Unit interactingUnit)
    {
        if(activeUnits.Contains(interactingUnit))
        {
            // Remove unit from this vent
            activeUnits.Remove(interactingUnit);
            interactingUnit.SetVisible(true);
            interactingUnit.SetState(UnitState.Idle);
            interactingUnit.LockRB(false);
        }
        else
        {
            // Add unit to this vent
            Enter(interactingUnit);
            interactingUnit.SetVisible(false);
            interactingUnit.SetState(UnitState.Null);
            interactingUnit.LockRB(true);
        }
        return true;
    }
    
    public void Enter(Unit unit) {
        activeUnits.Add(unit);
        unit.transform.position = transform.position;
        unit.data.input.Reset();
    }
    
    private void Update() {
        List<Unit> toRemove = new List<Unit>();
        foreach(Unit unit in activeUnits) {
            // Up
            if (unit.data.input.jumpQueued && upVent != null)
            {
                toRemove.Add(unit);
                upVent.Enter(unit);
            }
            // Down
            if (unit.data.input.crawling && downVent != null)
            {
                toRemove.Add(unit);
                downVent.Enter(unit);
            }
            // Right
            if (unit.data.input.movement == 1 && rightVent != null)
            {
                toRemove.Add(unit);
                rightVent.Enter(unit);
            }
            // Left
            if (unit.data.input.movement == -1 && leftVent != null)
            {
                toRemove.Add(unit);
                leftVent.Enter(unit);
            }
        }
        foreach(Unit unit in toRemove) {
            activeUnits.Remove(unit);
        }
    }
}
