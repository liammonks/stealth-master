using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : MonoBehaviour
{
    [SerializeField] protected List<UnitState> primaryAvailableStates, secondaryAvailableStates;
    
    protected Unit owner;
    protected bool primaryActive, secondaryActive;

    protected bool CanPrimary { get { return (!primaryActive && canPrimaryOverride) || (!primaryActive && primaryAvailableStates.Contains(owner.GetState()) && owner.data.t == 0.0f); } }
    protected bool canPrimaryOverride = false;
    protected bool CanSecondary { get { return (!secondaryActive && canSecondaryOverride) || (!secondaryActive && secondaryAvailableStates.Contains(owner.GetState()) && owner.data.t == 0.0f); } }
    protected bool canSecondaryOverride = false;

    public void Equip(Unit unit)
    {
        owner = unit;
    }
    
    public void EnablePrimary()
    {
        if (CanPrimary)
        {
            OnPrimaryEnabled();
            primaryActive = true;
        }
    }

    public void DisablePrimary()
    {
        if (!primaryActive) { return; }
        OnPrimaryDisabled();
        primaryActive = false;
    }

    public void EnableSecondary()
    {
        if (CanSecondary)
        {
            OnSecondaryEnabled();
            secondaryActive = true;
        }
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
