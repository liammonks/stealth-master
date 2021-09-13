using UnityEngine;

[CreateAssetMenu(fileName = "Jump", menuName = "States/Movement/Jump", order = 0)]
public class JumpState : MoveState
{
    [SerializeField] private MoveState Idle;
    [SerializeField] private MoveState Dive;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        animator.Play("Jump");
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        // Allow player to push towards movement speed while in the air
        float speed = data.input.running ? data.stats.runSpeed : data.stats.walkSpeed;
        if (Mathf.Abs(data.velocity.x) < speed)
        {
            data.velocity.x += speed * data.input.movement * Time.deltaTime * data.stats.airAuthority;
            data.velocity.x = Mathf.Clamp(data.velocity.x, -speed, speed);
        }
        
        // As long as the jump is queued, apply jump force
        if (data.input.jumpQueued)
        {
            data.velocity.y = data.stats.jumpForce;
        }
        
        // Execute Dive
        if(data.input.crawling)
        {
            data.collider.SetCrawling();
            return Dive;
        }
        
        // As soon as the player leaves the ground, the jump is no longer queued
        if((data.collision & UnitCollision.Ground) == 0)
        {
            data.input.jumpQueued = false;
        }
        else
        {
            // If the player is touching the ground and the jump is no longer queued, unit landed
            if(data.input.jumpQueued == false)
            {
                return Idle;
            }
        }
        return this;
    }

}
