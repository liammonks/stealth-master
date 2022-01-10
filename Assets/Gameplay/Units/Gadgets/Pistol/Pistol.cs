using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class Pistol : BaseGadget
    {
        [SerializeField] private BulletStats stats;
        [SerializeField] private Transform bulletSpawnPivot, bulletSpawn;

        private const float cameraOffsetDistance = 3.0f;

        private bool aiming = false;

        protected override void OnUnlocked()
        {
            base.OnUnlocked();
            OnAimPositionUpdated();
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
            aiming = true;
            if (owner == UnitHelper.Player)
            {
                PlayerCamera.Instance.SetState(PlayerCameraState.Aiming);
            }
        }

        protected override void OnSecondaryDisabled()
        {
            aiming = false;
            if (owner == UnitHelper.Player)
            {
                PlayerCamera.Instance.SetState(PlayerCameraState.Default);
            }
        }
        
        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (owner == UnitHelper.Player && aiming)
            {
                Vector2 cameraOffset = Vector2.ClampMagnitude(owner.AimOffset, cameraOffsetDistance);
                PlayerCamera.Instance.SetOffset(cameraOffset, true);
            }
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, owner.data.isFacingRight ? owner.AimOffset : -owner.AimOffset));
            bulletSpawnPivot.rotation = rotation;
            bulletSpawnPivot.position = owner.data.animator.GetLayer(UnitAnimatorLayer.FrontArm).transform.position;
        }

    }
}