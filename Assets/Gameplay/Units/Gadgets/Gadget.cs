using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : MonoBehaviour
{
    [SerializeField] protected List<UnitState> availableStates;
    protected Unit owner;
    protected bool primaryActive, secondaryActive;

    public void Equip(Unit unit) {
        owner = unit;
    }

    public abstract void PrimaryFunction(bool active);
    public abstract void SecondaryFunction(bool active);
}
