using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitAnimatorLayer
{
    Null,
    Body,
    FrontArm
}

public class UnitAnimator : MonoBehaviour
{
    public string CurrentState => lastState;

    public delegate void OnFacingUpdated();
    public event OnFacingUpdated onFacingUpdated;
    
    public bool reversed = false;

    [Header("Body")]
    [SerializeField] private Animator body;
    public RuntimeAnimatorController defaultBody;
    public RuntimeAnimatorController reversedBody;
    
    [Header("Front Arm")]
    [SerializeField] private Animator frontArm;
    public RuntimeAnimatorController defaultFrontArm;
    public RuntimeAnimatorController reversedFrontArm;

    private string lastState;
    private bool animationLocked;

    public void Play(UnitAnimatorLayer layer, string animation, bool forced = false)
    {
        if (animation == lastState) { return; }
        if(animationLocked && !forced)
        {
            if (onStateEnded != null) { StopCoroutine(onStateEnded); }
            onStateEnded = StartCoroutine(OnStateEndedCoroutine(layer, animation));
            return;
        }

        int id = Animator.StringToHash(animation);
        if (layer == UnitAnimatorLayer.Body)
        {
            body.Play(animation);
            lastState = animation;
            if (frontArm.runtimeAnimatorController == defaultFrontArm || frontArm.runtimeAnimatorController == reversedFrontArm)
            {
                frontArm.Play(animation);
            }
        }
        else if (layer == UnitAnimatorLayer.FrontArm)
        {
            frontArm.Play(animation);
        }

        body.Update(0);
        frontArm.Update(0);

        if (forced) { animationLocked = true; }
    }

    private Coroutine onStateEnded;
    private IEnumerator OnStateEndedCoroutine(UnitAnimatorLayer layer, string animation)
    {
        UpdateState();
        yield return new WaitForSeconds(Mathf.Lerp(GetState().length, 0, GetState().normalizedTime));
        animationLocked = false;
        Play(layer, animation);
    }
    
    public void SetLayer(UnitAnimatorLayer layer, RuntimeAnimatorController controller)
    {
        Animator animator = GetLayer(layer);
        if (animator.runtimeAnimatorController == controller) return;
        int state = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animator.runtimeAnimatorController = controller;
        if (animator.HasState(0, state)) animator.Play(state, 0, normalizedTime);
    }
    
    public Animator GetLayer(UnitAnimatorLayer layer)
    {
        switch (layer)
        {
            case UnitAnimatorLayer.Body:
                return body;
            case UnitAnimatorLayer.FrontArm:
                return frontArm;
        }
        return null;
    }
    
    public void RotateLayer(UnitAnimatorLayer layer, Quaternion rotation)
    {
        switch(layer)
        {
            case UnitAnimatorLayer.Body:
                body.transform.rotation = rotation;
                break;
            case UnitAnimatorLayer.FrontArm:
                frontArm.transform.rotation = rotation;
                break;
        }
    }
    
    public void SetVisible(bool isVisible)
    {
        body.gameObject.SetActive(isVisible);
        frontArm.gameObject.SetActive(isVisible);
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
        frontArm.SetFloat("VelocityX", velocity);
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
    }
    
}
