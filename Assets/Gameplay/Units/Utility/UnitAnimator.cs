using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitAnimatorLayer
{
    Null,
    Body,
    FrontArm,
    BackArm
}

public class UnitAnimator : MonoBehaviour
{
    public string CurrentState => lastState;

    public delegate void OnFacingUpdated();
    public event OnFacingUpdated onFacingUpdated;
    
    public bool reversed = false;

    [Header("Controllers")]
    public RuntimeAnimatorController defaultBody;
    public RuntimeAnimatorController reversedBody;
    
    [Header("Defaults")]
    public RuntimeAnimatorController defaultFrontArm;
    public RuntimeAnimatorController reversedFrontArm;

    [Header("Animators")]
    [SerializeField] private Animator body;
    [SerializeField] private Animator frontArm;
    [SerializeField] private Animator backArm;

    private string lastState;
    private bool animationLocked;

    public void Play(string animation, bool forced = false, UnitAnimatorLayer layer = UnitAnimatorLayer.Null)
    {
        if (animation == lastState) { return; }
        if(animationLocked && !forced)
        {
            if (onStateEnded != null) { StopCoroutine(onStateEnded); }
            onStateEnded = StartCoroutine(OnStateEndedCoroutine(animation));
            return;
        }

        int id = Animator.StringToHash(animation);
        if((layer == UnitAnimatorLayer.Null || layer == UnitAnimatorLayer.Body) && body.HasState(0, id)) body.Play(animation);
        if((layer == UnitAnimatorLayer.Null || layer == UnitAnimatorLayer.FrontArm) && frontArm.HasState(0, id)) frontArm.Play(animation);
        //if(layer == UnitAnimatorLayer.Null || layer == UnitAnimatorLayer.BackArm && backArm.HasState(0, id)) backArm.Play(animation);

        body.Update(0);
        frontArm.Update(0);
        //backArm.Update(0);

        lastState = animation;
        if (forced) { animationLocked = true; }
    }

    private Coroutine onStateEnded;
    private IEnumerator OnStateEndedCoroutine(string animation)
    {
        UpdateState();
        yield return new WaitForSeconds(Mathf.Lerp(GetState().length, 0, GetState().normalizedTime));
        animationLocked = false;
        Play(animation);
    }
    
    public void SetLayer(UnitAnimatorLayer layer, RuntimeAnimatorController controller)
    {
        float normalizedTime = GetState().normalizedTime;
        int state;
        switch (layer)
        {
            case UnitAnimatorLayer.Body:
                if (body.runtimeAnimatorController == controller) return;
                state = body.GetCurrentAnimatorStateInfo(0).fullPathHash;
                body.runtimeAnimatorController = controller;
                body.Play(body.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, normalizedTime);
                break;
            case UnitAnimatorLayer.FrontArm:
                if (frontArm.runtimeAnimatorController == controller) return;
                state = frontArm.GetCurrentAnimatorStateInfo(0).fullPathHash;
                frontArm.runtimeAnimatorController = controller;
                if (frontArm.HasState(0, state)) frontArm.Play(state, 0, normalizedTime);
                break;
            case UnitAnimatorLayer.BackArm:
                //if (backArm.runtimeAnimatorController == controller) return;
                //backArm.runtimeAnimatorController = controller;
                //backArm.Play(GetState().fullPathHash, 0, normalizedTime);
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
                //backArm.transform.rotation = rotation;
                break;
        }
    }
    
    public void SetVisible(bool isVisible)
    {
        body.gameObject.SetActive(isVisible);
        frontArm.gameObject.SetActive(isVisible);
        //backArm.gameObject.SetActive(isVisible);
    }

    public void SetFacing(bool facingRight)
    {
        Vector3 lastScale = transform.parent.localScale;
        transform.parent.localScale = facingRight ? Vector3.one : new Vector3(-1, 1, 1);
        if(transform.parent.localScale != lastScale) onFacingUpdated?.Invoke();
    }
    
    public void SetVelocity(float velocity)
    {
        velocity = Mathf.Abs(velocity);
        body.SetFloat("VelocityX", velocity);
        //frontArm.SetFloat("VelocityX", velocity);
        //backArm.SetFloat("VelocityX", velocity);
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
        
        //backArm.Update(0);
        //backArm.Update(0);
    }
    
}
