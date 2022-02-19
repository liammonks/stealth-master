using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class Pistol : BaseGadget
    {
        [Header("Pistol")]
        [SerializeField] private BulletStats stats;
        [SerializeField] private Transform bulletSpawnForward, bulletSpawnBackward;

        private Transform bulletSpawn => owner.AimingBehind() ? bulletSpawnBackward : bulletSpawnForward;

        private const float cameraOffsetDistance = 3.0f;

        private bool aiming = false;

        protected override void Unholster()
        {
            base.Unholster();
            OnAimPositionUpdated();
        }

        protected override void OnPrimaryEnabled()
        {
            BulletPool.Fire(bulletSpawn.position, owner.data.isFacingRight ? bulletSpawn.right : -bulletSpawn.right, owner.data.rb.velocity, stats, owner is Player);
            owner.data.animator.Play(UnitAnimatorLayer.FrontArm, "Shoot");
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
        
        private void FixedUpdate() {
            if (owner == UnitHelper.Player && aiming)
            {
                Vector2 cameraOffset = Vector2.ClampMagnitude(owner.AimOffset, cameraOffsetDistance);
                PlayerCamera.Instance.SetOffset(cameraOffset, true);
            }
        }

    }
}