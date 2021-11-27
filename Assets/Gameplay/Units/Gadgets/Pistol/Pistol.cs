using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gadget
{
    private const float cameraOffsetDistance = 2.0f;
    
    private bool aiming = false;

    protected override void OnPrimaryDisabled()
    {
        
    }

    protected override void OnPrimaryEnabled()
    {
        Debug.DrawLine(transform.position, Camera.main.ScreenToWorldPoint(Player.MousePosition), Color.red, 1.0f);
    }

    protected override void OnSecondaryDisabled()
    {
        owner.SetState(UnitState.Idle);
        canPrimaryOverride = false;
        aiming = false;
        if (owner == UnitHelper.Player)
        {
            UnitHelper.Player.SetCameraOffset(Vector2.zero);
        }
    }

    protected override void OnSecondaryEnabled()
    {
        owner.data.animator.Play("Idle");
        owner.SetState(UnitState.Null);
        canPrimaryOverride = true;
        aiming = true;
    }
    
    private void Update() {
        if (aiming && owner == UnitHelper.Player)
        {
            Vector2 mouseOffset = Camera.main.ScreenToWorldPoint(Player.MousePosition) - UnitHelper.Player.transform.position;
            mouseOffset = Vector2.ClampMagnitude(mouseOffset, cameraOffsetDistance);
            UnitHelper.Player.SetCameraOffset(mouseOffset);
            owner.data.isFacingRight = mouseOffset.x > 0;
            transform.localPosition = mouseOffset.normalized * 0.5f;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, mouseOffset));
            transform.localScale = owner.data.isFacingRight ? Vector3.one : new Vector3(1, -1, 1);
        }
    }
}
