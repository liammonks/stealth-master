using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gadgets
{
    public class Pistol : BaseGadget
    {
        [SerializeField] private BulletStats stats;
        [SerializeField] private Transform pivot, bulletSpawn;
        
        private const float cameraOffsetDistance = 3.0f;

        private bool aiming = false;

        protected override void OnPrimaryEnabled()
        {
            Vector2 direction = UnityEngine.Camera.main.ScreenToWorldPoint(Player.MousePosition) - transform.position;
            BulletPool.Fire(bulletSpawn.position, direction, owner.data.rb.velocity, stats, owner is Player);
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

        private void Update()
        {
            if (owner == UnitHelper.Player)
            {
                Vector2 mouseOffset = UnityEngine.Camera.main.ScreenToWorldPoint(Player.MousePosition) - UnitHelper.Player.transform.position;
                mouseOffset = Vector2.ClampMagnitude(mouseOffset, cameraOffsetDistance);
                
                if (aiming) {
                    Vector2 cameraOffset = Vector2.ClampMagnitude(mouseOffset, cameraOffsetDistance);
                    PlayerCamera.Instance.SetOffset(cameraOffset, true);
                }
                
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, owner.data.isFacingRight ? mouseOffset : -mouseOffset));
                owner.data.animator.RotateLayer(UnitAnimatorLayer.FrontArm, rotation);
                pivot.rotation = rotation;
            }
        }

    }
}