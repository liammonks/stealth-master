using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "States/Movement/Idle", order = 0)]
public class IdleState : MoveState
{
    [SerializeField] private MoveState Run;
    [SerializeField] private MoveState Jump;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        animator.Play("Idle");
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        // Execute Jump
        if (data.ShouldJump())
        {
            return Jump;
        }
        
        // Execute run state
        if (Mathf.Abs(data.input.movement) > 0)
        {
            return Run;
        }

        return this;
    }
}