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
    public string CurrentState => lastState;

    [SerializeField] private Animator body, frontArm, backArm;

    private string lastState;
    private bool animationLocked;

    public void Play(string animation, bool forced = false)
    {
        if (animation == lastState) { return; }
        
        if(animationLocked && !forced)
        {
            if (onStateEnded != null) { StopCoroutine(onStateEnded); }
            onStateEnded = StartCoroutine(OnStateEndedCoroutine(animation));
            return;
        }

        body.Update(0);
        frontArm.Update(0);
        backArm.Update(0);

        body.Play(animation);
        frontArm.Play(animation);
        backArm.Play(animation);

        lastState = animation;
        if (forced) { animationLocked = true; }
    }

    private Coroutine onStateEnded;
    private IEnumerator OnStateEndedCoroutine(string animation)
    {
        yield return new WaitForSeconds(Mathf.Lerp(0, GetState().length, GetState().normalizedTime));
        animationLocked = false;
        Play(animation);
    }
    
    public void SetLayer(UnitAnimatorLayer layer, RuntimeAnimatorController controller)
    {
        float normalizedTime = GetState().normalizedTime;
        switch (layer)
        {
            case UnitAnimatorLayer.FrontArm:
                frontArm.runtimeAnimatorController = controller;
                frontArm.Play(lastState, 0, normalizedTime);
                break;
            case UnitAnimatorLayer.BackArm:
                backArm.runtimeAnimatorController = controller;
                backArm.Play(lastState, 0, normalizedTime);
                break;
        }
    }
    
    public Animator GetLayer(UnitAnimatorLayer layer)
    {
        switch (layer)
        {
            case UnitAnimatorLayer.FrontArm:
                return frontArm;
            case UnitAnimatorLayer.BackArm:
                return backArm;
        }
        return null;
    }
    
    public void RotateLayer(UnitAnimatorLayer layer, Quaternion rotation)
    {
        switch(layer)
        {
            case UnitAnimatorLayer.FrontArm:
                frontArm.transform.rotation = rotation;
                break;
            case UnitAnimatorLayer.BackArm:
                backArm.transform.rotation = rotation;
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
        transform.parent.localScale = facingRight ? Vector3.one : new Vector3(-1, 1, 1);
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
    
    public void UpdateState()
    {
        body.Update(0);
        body.Update(0);

        frontArm.Update(0);
        frontArm.Update(0);

        backArm.Update(0);
        backArm.Update(0);
    }
    
}
