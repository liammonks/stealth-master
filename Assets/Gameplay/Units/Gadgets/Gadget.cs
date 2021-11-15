using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : MonoBehaviour
{
    [SerializeField] protected List<UnitState> primaryAvailableStates, secondaryAvailableStates;
    
    protected Unit owner;
    protected bool primaryActive, secondaryActive;
    
    public void Equip(Unit unit)
    {
        owner = unit;
    }
    
    public void EnablePrimary()
    {
        if (primaryActive) { return; }
        if (!primaryAvailableStates.Contains(owner.GetState())) { return; }
        // Current state must not be in transition
        if (owner.data.t != 0.0f) { return; }
        OnPrimaryEnabled();
        primaryActive = true;
    }

    public void DisablePrimary()
    {
        if (!primaryActive) { return; }
        OnPrimaryDisabled();
        primaryActive = false;
    }

    public void EnableSecondary()
    {
        if (secondaryActive) { return; }
        if (!secondaryAvailableStates.Contains(owner.GetState())) { return; }
        // Current state must not be in transition
        if (owner.data.t != 0.0f) { return; }
        OnSecondaryEnabled();
        secondaryActive = true;
    }

    public void DisableSecondary()
    {
        if (!secondaryActive) { return; }
        OnSecondaryDisabled();
        secondaryActive = false;
    }

    protected abstract void OnPrimaryEnabled();
    protected abstract void OnPrimaryDisabled();

    protected abstract void OnSecondaryEnabled();
    protected abstract void OnSecondaryDisabled();
    
}
