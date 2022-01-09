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
        [SerializeField] protected RuntimeAnimatorController frontArmAnimatorController;
        [SerializeField] protected RuntimeAnimatorController backArmAnimatorController;

        protected Unit owner;
        protected bool primaryActive, secondaryActive;

        protected bool CanPrimary { get { return (!primaryActive && canPrimaryOverride) || (!primaryActive && primaryAvailableStates.Contains(owner.GetState())); } }
        protected bool canPrimaryOverride = false;
        protected bool CanSecondary { get { return (!secondaryActive && canSecondaryOverride) || (!secondaryActive && secondaryAvailableStates.Contains(owner.GetState())); } }
        protected bool canSecondaryOverride = false;

        public void Equip(Unit unit)
        {
            owner = unit;
            unit.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, frontArmAnimatorController);
            unit.data.animator.SetLayer(UnitAnimatorLayer.BackArm, backArmAnimatorController);
            OnEquip();
            OnUnitStateUpdated(unit.GetState());
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
        
        public void OnUnitStateUpdated(UnitState state)
        {
            if(primaryAvailableStates.Contains(state) || secondaryAvailableStates.Contains(state))
            {
                gameObject.SetActive(true);
            }
            else
            {
                DisablePrimary();
                DisableSecondary();
                gameObject.SetActive(false);
            }
        }

        protected virtual void OnEquip() { }

        protected abstract void OnPrimaryEnabled();
        protected abstract void OnPrimaryDisabled();

        protected abstract void OnSecondaryEnabled();
        protected abstract void OnSecondaryDisabled();

    }
}