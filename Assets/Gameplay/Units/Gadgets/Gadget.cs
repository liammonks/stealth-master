using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public abstract class BaseGadget : MonoBehaviour
    {
        public bool PrimaryActive { get => primaryActive; }
        public bool SecondaryActive { get => secondaryActive; }

        [SerializeField] protected List<UnitState> primaryAvailableStates, secondaryAvailableStates;

        protected Unit owner;
        protected bool primaryActive, secondaryActive;

        protected bool CanPrimary { get { return (!primaryActive && canPrimaryOverride) || (!primaryActive && primaryAvailableStates.Contains(owner.GetState())); } }
        protected bool canPrimaryOverride = false;
        protected bool CanSecondary { get { return (!secondaryActive && canSecondaryOverride) || (!secondaryActive && secondaryAvailableStates.Contains(owner.GetState())); } }
        protected bool canSecondaryOverride = false;

        public void Equip(Unit unit)
        {
            owner = unit;
            OnEquip();
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

        protected virtual void OnEquip() { }

        protected abstract void OnPrimaryEnabled();
        protected abstract void OnPrimaryDisabled();

        protected abstract void OnSecondaryEnabled();
        protected abstract void OnSecondaryDisabled();

    }
}