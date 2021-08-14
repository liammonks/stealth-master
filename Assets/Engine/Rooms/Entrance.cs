using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Entrance : Interactable
{

    [SerializeField] private Transform enterPoint, exitPoint;
    private Room room;

    public void Initialise(Room parentRoom)
    {
        room = parentRoom;
    }

    public override void OnInteract(Unit interactingUnit)
    {
        if(!room.ContainsUnit(interactingUnit))
        {
            // Room does not contain interacting unit
            Enter(interactingUnit);
        }
        else
        {
            // Interacting unit is already inside the room
            Exit(interactingUnit);
        }
    }
    
    public void Enter(Unit unit)
    {
        unit.transform.position = enterPoint.position;
        room.OnUnitEnteredRoom(unit);
    }
    
    public void Exit(Unit unit)
    {
        unit.transform.position = exitPoint.position;
        room.OnUnitExitedRoom(unit);
    }
}
