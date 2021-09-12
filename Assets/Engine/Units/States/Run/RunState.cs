using UnityEngine;

[CreateAssetMenu(fileName = "Run", menuName = "States/Movement/Run", order = 0)]
public class RunState : MoveState
{
    [SerializeField] private MoveState Idle;
    [SerializeField] private MoveState Jump;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        animator.Play("Run");
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        // Apply movement input
        if (Mathf.Abs(data.velocity.x) < data.stats.runSpeed)
        {
            float speed = data.input.running ? data.stats.runSpeed : data.stats.walkSpeed;
            data.velocity.x += speed * data.input.movement * Time.deltaTime * data.stats.groundAuthority;
            data.velocity.x = Mathf.Clamp(data.velocity.x, -speed, speed);
        }
        
        // Execute Jump
        if (data.input.jumpQueued)
        {
            return Jump;
        }
        
        // Return to Idle when stopped
        if(data.velocity.magnitude == 0)
        {
            return Idle;
        }
        
        return this;
    }

}
