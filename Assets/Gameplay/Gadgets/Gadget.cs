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
        
        [Header("Front Arm")]
        [SerializeField] protected RuntimeAnimatorController frontArmAnimatorController;
        [SerializeField] protected RuntimeAnimatorController frontArmAnimatorControllerReversed;

        [Header("Visuals")]
        [SerializeField] protected Transform visualsRoot;
        [SerializeField] protected Transform forwardVisuals;
        [SerializeField] protected Transform reverseVisuals;

        [Header("Colliders")]
        [SerializeField] protected Transform collidersRoot;
        [SerializeField] protected Transform forwardCollider;
        [SerializeField] protected Transform reverseCollider;

        [Header("Mods")]
        [SerializeField] private List<GadgetMod> mods;

        protected Unit owner;
        protected bool primaryActive, secondaryActive;
        protected bool primaryLocked, secondaryLocked;
        protected bool rotationLocked = false;
        protected bool holstered = false;

        protected bool CanPrimary { get { return !holstered && !primaryActive && primaryAvailableStates.Contains(owner.GetState()) && !primaryLocked && !rotationLocked; } }
        protected bool CanSecondary { get { return !holstered && !secondaryActive && secondaryAvailableStates.Contains(owner.GetState()) && !secondaryLocked && !rotationLocked; } }

        private List<GameObject> intersectingObjects = new List<GameObject>();

        private const float raycastDistance = 0.8f;

        public void Equip(Unit unit)
        {
            owner = unit;

            owner.onAimOffsetUpdated += OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated += OnAimPositionUpdated;
            owner.data.lockGadget += Holster;
            owner.stateMachine.onStateUpdated += OnUnitStateUpdated;
            
            OnEquip();
            OnUnitStateUpdated(unit.GetState());
            OnAimPositionUpdated();

            foreach (GadgetMod mod in mods)
            {
                mod.Activate(this);
            }
        }

        private void OnDestroy()
        {
            owner.onAimOffsetUpdated -= OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated -= OnAimPositionUpdated;
            owner.data.lockGadget -= Holster;
            owner.stateMachine.onStateUpdated -= OnUnitStateUpdated;
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
                Unholster();
            }
            else
            {
                Holster();
            }
        }

        private void Update()
        {
            transform.position = owner.data.animator.GetLayer(UnitAnimatorLayer.FrontArm).transform.position;
        }

        protected virtual void OnAimPositionUpdated()
        {
            if (holstered) return;
            bool aimingBehind = owner.AimingBehind();
            owner.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, aimingBehind ? frontArmAnimatorControllerReversed : frontArmAnimatorController);
            //owner.data.animator.SetLayer(UnitAnimatorLayer.BackArm, aimingBehind ? backArmAnimatorControllerReversed : backArmAnimatorController);
            if (!rotateFrontArm) return;
            
            forwardVisuals.gameObject.SetActive(!aimingBehind);
            forwardCollider.gameObject.SetActive(!aimingBehind);
            reverseVisuals.gameObject.SetActive(aimingBehind);
            reverseCollider.gameObject.SetActive(aimingBehind);
            collidersRoot.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, owner.data.isFacingRight ? owner.AimOffset : -owner.AimOffset));
            if (rotationLocked)
            {
                if (owner.data.isStanding)
                {
                    // Lock aim down
                    visualsRoot.rotation = Quaternion.LookRotation(Vector3.forward, owner.data.isFacingRight ? Vector3.right : Vector3.left);
                }
                else
                {
                    // Lock aim left/right
                    visualsRoot.rotation = Quaternion.LookRotation(Vector3.forward, aimingBehind ? Vector3.down : Vector3.up);
                }
            }
            else
            {
                visualsRoot.rotation = collidersRoot.rotation;
            }
            owner.data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, visualsRoot.rotation);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger) return;
            intersectingObjects.Add(other.gameObject);
            rotationLocked = true;
            OnAimPositionUpdated();
        }
        
        private void OnTriggerExit2D(Collider2D other) {
            if (other.isTrigger) return;
            intersectingObjects.Remove(other.gameObject);
            if(intersectingObjects.Count == 0)
            {
                rotationLocked = false;
                OnAimPositionUpdated();
            }
        }
        
        protected virtual void Holster()
        {
            holstered = true;
            DisablePrimary();
            DisableSecondary();
            gameObject.SetActive(false);
            owner.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, owner.AimingBehind() ? owner.data.animator.reversedFrontArm : owner.data.animator.defaultFrontArm);
        }

        protected virtual void Unholster()
        {
            holstered = false;
            gameObject.SetActive(true);
            owner.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, owner.AimingBehind() ? frontArmAnimatorControllerReversed : frontArmAnimatorController);
        }

        protected virtual void OnEquip() { }

        protected abstract void OnPrimaryEnabled();
        protected abstract void OnPrimaryDisabled();

        protected abstract void OnSecondaryEnabled();
        protected abstract void OnSecondaryDisabled();

    }
}