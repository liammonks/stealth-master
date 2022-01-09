using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class Pistol : BaseGadget
    {
        [SerializeField] private BulletStats stats;
        [SerializeField] private Transform bulletSpawnPivot, bulletSpawn;
        
        [SerializeField] private RuntimeAnimatorController frontArmAnimatorControllerReversed;
        [SerializeField] private RuntimeAnimatorController backArmAnimatorControllerReversed;

        private const float cameraOffsetDistance = 3.0f;

        private bool aiming = false;

        protected override void OnEquip()
        {
            owner.onAimOffsetUpdated += OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated += OnAimPositionUpdated;
        }
        
        private void OnDestroy() {
            owner.onAimOffsetUpdated -= OnAimPositionUpdated;
            owner.data.animator.onFacingUpdated -= OnAimPositionUpdated;
        }

        protected override void OnPrimaryEnabled()
        {
            BulletPool.Fire(bulletSpawn.position, owner.AimOffset, owner.data.rb.velocity, stats, owner is Player);
            owner.data.animator.Play("Shoot", false, UnitAnimatorLayer.FrontArm);
        }
        
        protected override void OnPrimaryDisabled()
        {
            
        }

        protected override void OnSecondaryEnabled()
        {
            canPrimaryOverride = true;
            aiming = true;
            if (owner == UnitHelper.Player)
            {
                PlayerCamera.Instance.SetState(PlayerCameraState.Aiming);
            }
        }

        protected override void OnSecondaryDisabled()
        {
            canPrimaryOverride = false;
            aiming = false;
            if (owner == UnitHelper.Player)
            {
                PlayerCamera.Instance.SetState(PlayerCameraState.Default);
            }
        }

        private void OnAimPositionUpdated()
        {  
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, owner.data.isFacingRight ? owner.AimOffset : -owner.AimOffset));
            owner.data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, rotation);
            bulletSpawnPivot.rotation = rotation;
            bulletSpawnPivot.position = owner.data.animator.GetLayer(UnitAnimatorLayer.FrontArm).transform.position;

            bool aimingBehind = owner.AimingBehind();
            owner.data.animator.SetLayer(UnitAnimatorLayer.FrontArm, aimingBehind ? frontArmAnimatorControllerReversed : frontArmAnimatorController, aimingBehind);
            owner.data.animator.SetLayer(UnitAnimatorLayer.BackArm, aimingBehind ? backArmAnimatorControllerReversed : backArmAnimatorController);
        }
        
        private void Update() {
            if (owner == UnitHelper.Player && aiming)
            {
                Vector2 cameraOffset = Vector2.ClampMagnitude(owner.AimOffset, cameraOffsetDistance);
                PlayerCamera.Instance.SetOffset(cameraOffset, true);
            }
        }

    }
}