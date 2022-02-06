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
        [Header("Back Arm")]
        [SerializeField] protected RuntimeAnimatorController backArmAnimatorController;
        [SerializeField] protected RuntimeAnimatorController backArmAnimatorControllerReversed;

        [Header("Visuals")]
        [SerializeField] private Transform visualsRoot;
        [SerializeField] private Transform forwardVisuals;
        [SerializeField] private Transform reverseVisuals;

        [Header("Colliders")]
        [SerializeField] private Transform collidersRoot;
        [SerializeField] private Transform forwardCollider;
        [SerializeField] private Transform reverseCollider;

        protected Unit owner;
        protected bool primaryActive, secondaryActive;
        protected bool primaryLocked, secondaryLocked;

        protected bool CanPrimary { get { return !primaryActive && primaryAvailableStates.Contains(owner.GetState()) && !primaryLocked && !rotationLocked; } }
        protected bool CanSecondary { get { return !secondaryActive && secondaryAvailableStates.Contains(owner.GetState()) && !secondaryLocked && !rotationLocked; } }

        private bool previouslyAimingBehind = false;
        private List<GameObject> intersectingObjects = new List<GameObject>();
        private bool rotationLocked = false;

        private const float raycastDistance = 0.8f;

        public void Equip(Unit unit)
        {
            owner = unit;
            unit.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, frontArmAnimatorController);
            unit.data.animator.SetLayer(UnitAnimatorLayer.BackArm, backArmAnimatorController);

            owner.onAimOffsetUpdated += OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated += OnAimPositionUpdated;
            owner.data.lockGadget += OnLocked;
            owner.stateMachine.onStateUpdated += OnUnitStateUpdated;

            OnEquip();
            OnUnitStateUpdated(unit.GetState());
        }

        private void OnDestroy()
        {
            owner.onAimOffsetUpdated -= OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated -= OnAimPositionUpdated;
            owner.data.lockGadget -= OnLocked;
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
                OnUnlocked();
            }
            else
            {
                OnLocked();
            }
        }

        protected virtual void FixedUpdate()
        {
            Vector2 armPivot = owner.data.animator.GetLayer(UnitAnimatorLayer.FrontArm).transform.position;
            transform.position = armPivot;
        }

        protected virtual void OnAimPositionUpdated()
        {
            bool aimingBehind = owner.AimingBehind();
            owner.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, aimingBehind ? frontArmAnimatorControllerReversed : frontArmAnimatorController, rotateFrontArm && aimingBehind);
            owner.data.animator.SetLayer(UnitAnimatorLayer.BackArm, aimingBehind ? backArmAnimatorControllerReversed : backArmAnimatorController);
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
            intersectingObjects.Add(other.gameObject);
            rotationLocked = true;
            OnAimPositionUpdated();
        }
        
        private void OnTriggerExit2D(Collider2D other) {
            intersectingObjects.Remove(other.gameObject);
            if(intersectingObjects.Count == 0)
            {
                rotationLocked = false;
            }
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

        protected abstract void OnPrimaryEnabled();
        protected abstract void OnPrimaryDisabled();

        protected abstract void OnSecondaryEnabled();
        protected abstract void OnSecondaryDisabled();

    }
}