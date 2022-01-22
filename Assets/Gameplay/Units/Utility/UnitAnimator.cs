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

    [Header("Controllers")]
    public RuntimeAnimatorController defaultBody;
    public RuntimeAnimatorController reversedBody;
    public RuntimeAnimatorController defaultFrontArm;
    public RuntimeAnimatorController defaultBackArm;

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

        if(layer == UnitAnimatorLayer.Null || layer == UnitAnimatorLayer.Body) body.Play(animation);
        if(layer == UnitAnimatorLayer.Null || layer == UnitAnimatorLayer.FrontArm) frontArm.Play(animation);
        if(layer == UnitAnimatorLayer.Null || layer == UnitAnimatorLayer.BackArm) backArm.Play(animation);

        body.Update(0);
        frontArm.Update(0);
        backArm.Update(0);

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
    
    public void SetLayer(UnitAnimatorLayer layer, RuntimeAnimatorController controller, bool inverted = false)
    {
        float normalizedTime = GetState().normalizedTime;
        switch (layer)
        {
            case UnitAnimatorLayer.Body:
                if (body.runtimeAnimatorController == controller) return;
                body.transform.localScale = inverted ? new Vector3(-1.0f, -1.0f, 1.0f) : Vector3.one;
                body.runtimeAnimatorController = controller;
                body.Play(GetState().fullPathHash, 0, normalizedTime);
                break;
            case UnitAnimatorLayer.FrontArm:
                if (frontArm.runtimeAnimatorController == controller) return;
                frontArm.transform.localScale = inverted ? new Vector3(-1.0f, -1.0f, 1.0f) : Vector3.one;
                frontArm.runtimeAnimatorController = controller;
                frontArm.Play(GetState().fullPathHash, 0, normalizedTime);
                break;
            case UnitAnimatorLayer.BackArm:
                if (backArm.runtimeAnimatorController == controller) return;
                backArm.transform.localScale = inverted ? new Vector3(-1.0f, -1.0f, 1.0f) : Vector3.one;
                backArm.runtimeAnimatorController = controller;
                backArm.Play(GetState().fullPathHash, 0, normalizedTime);
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
        Vector3 lastScale = transform.parent.localScale;
        transform.parent.localScale = facingRight ? Vector3.one : new Vector3(-1, 1, 1);
        if(transform.parent.localScale != lastScale) onFacingUpdated?.Invoke();
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
