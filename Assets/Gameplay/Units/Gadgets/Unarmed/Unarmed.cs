using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unarmed : Gadget
{

    public override void PrimaryFunction()
    {
        // Must be Idle or Running to Punch
        if (owner.GetState() != UnitState.Idle && owner.GetState() != UnitState.Run) { return; }
        owner.SetState(UnitState.Null);
        owner.data.animator.Play("Punch");
        owner.data.animator.Update(0);
        owner.data.animator.Update(0);
        owner.data.rb.velocity = Vector2.zero;
        StartCoroutine(ToIdle(owner.data.animator.GetCurrentAnimatorStateInfo(0).length));
    }

    public override void SecondaryFunction()
    {
        
    }
    
    private IEnumerator ToIdle(float delay) {
        yield return new WaitForSeconds(delay);
        owner.SetState(UnitState.Idle);
    }
}
