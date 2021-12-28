using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitAnimatorLayer
{
    Body,
    FrontArm,
    BackArm
}

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator body, frontArm, backArm;
    [SerializeField] private Transform frontArmPivot, backArmPivot;

    private string lastState;

    public void Play(string animation)
    {
        if (animation == lastState) { return; }
        lastState = animation;
        body.Play(animation);
        frontArm.Play(animation);
        backArm.Play(animation);
    }
    
    public void SetLayer(UnitAnimatorLayer layer, RuntimeAnimatorController controller)
    {
        switch (layer)
        {
            case UnitAnimatorLayer.FrontArm:
                frontArm.runtimeAnimatorController = controller;
                break;
            case UnitAnimatorLayer.BackArm:
                backArm.runtimeAnimatorController = controller;
                break;
        }
    }
    
    public void RotateLayer(UnitAnimatorLayer layer, Quaternion rotation)
    {
        switch(layer)
        {
            case UnitAnimatorLayer.FrontArm:
                frontArmPivot.rotation = rotation;
                break;
            case UnitAnimatorLayer.BackArm:
                backArmPivot.rotation = rotation;
                break;
        }
    }
    
    public void SetVisible(bool isVisible)
    {
        body.gameObject.SetActive(isVisible);
        frontArm.gameObject.SetActive(isVisible);
        backArm.gameObject.SetActive(isVisible);
    }

    public void SetFacing(bool facingRight)
    {
        transform.localScale = facingRight ? Vector3.one : new Vector3(-1, 1, 1);
    }
    
    public void SetVelocity(float velocity)
    {
        velocity = Mathf.Abs(velocity);
        body.SetFloat("VelocityX", velocity);
        frontArm.SetFloat("VelocityX", velocity);
        backArm.SetFloat("VelocityX", velocity);
    }

    public AnimatorStateInfo GetState()
    {
        return body.GetCurrentAnimatorStateInfo(0);
    }
}
