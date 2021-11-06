using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : MonoBehaviour
{
    [SerializeField] protected List<UnitState> availableStates;
    protected Unit owner;

    public void Equip(Unit unit) {
        owner = unit;
    }

    public abstract void PrimaryFunction();
    public abstract void SecondaryFunction();
}
