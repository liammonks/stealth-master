using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public abstract class BaseGadget : MonoBehaviour
    {

        public bool PrimaryActive { get => primaryActive; }
        public bool SecondaryActive { get => secondaryActive; }

        [SerializeField] protected bool rotateFrontArm = false;
        [SerializeField] protected List<UnitState> primaryAvailableStates, secondaryAvailableStates;
        [SerializeField] protected RuntimeAnimatorController frontArmAnimatorController;
        [SerializeField] protected RuntimeAnimatorController backArmAnimatorController;
        [SerializeField] protected RuntimeAnimatorController frontArmAnimatorControllerReversed;
        [SerializeField] protected RuntimeAnimatorController backArmAnimatorControllerReversed;

        protected Unit owner;
        protected bool primaryActive, secondaryActive;
        protected bool primaryLocked, secondaryLocked;

        protected bool CanPrimary { get { return !primaryActive && primaryAvailableStates.Contains(owner.GetState()) && !primaryLocked; } }
        protected bool CanSecondary { get { return !secondaryActive && secondaryAvailableStates.Contains(owner.GetState()) && !secondaryLocked; } }

        private bool previouslyAimingBehind = false;

        public void Equip(Unit unit)
        {
            owner = unit;
            unit.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, frontArmAnimatorController);
            unit.data.animator.SetLayer(UnitAnimatorLayer.BackArm, backArmAnimatorController);
            owner.onAimOffsetUpdated += OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated += OnAimPositionUpdated;
            owner.data.lockGadget += OnLocked;
            OnEquip();
            OnUnitStateUpdated(unit.GetState());
        }

        private void OnDestroy()
        {
            owner.onAimOffsetUpdated -= OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated -= OnAimPositionUpdated;
            owner.data.lockGadget -= OnLocked;
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
                OnUnlocked();
            }
            else
            {
                OnLocked();
            }
        }

        protected virtual void FixedUpdate() {
            if (rotateFrontArm)
            {
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, owner.data.isFacingRight ? owner.AimOffset : -owner.AimOffset));
                owner.data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, rotation);
            }

            bool aimingBehind = owner.AimingBehind();
            owner.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, aimingBehind ? frontArmAnimatorControllerReversed : frontArmAnimatorController, rotateFrontArm && aimingBehind);
            owner.data.animator.SetLayer(UnitAnimatorLayer.BackArm, aimingBehind ? backArmAnimatorControllerReversed : backArmAnimatorController);
        }
        
        protected virtual void OnLocked()
        {
            primaryLocked = true;
            secondaryLocked = true;
            DisablePrimary();
            DisableSecondary();
            gameObject.SetActive(false);
        }

        protected virtual void OnUnlocked()
        {
            primaryLocked = false;
            secondaryLocked = false;
            gameObject.SetActive(true);
        }

        protected virtual void OnEquip() { }
        protected virtual void OnAimPositionUpdated() { }

        protected abstract void OnPrimaryEnabled();
        protected abstract void OnPrimaryDisabled();

        protected abstract void OnSecondaryEnabled();
        protected abstract void OnSecondaryDisabled();

    }
}