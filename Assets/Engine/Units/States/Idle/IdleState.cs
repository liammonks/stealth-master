using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "States/Movement/Idle", order = 0)]
public class IdleState : MoveState
{
    [SerializeField] private MoveState Run;
    [SerializeField] private MoveState Jump;
    [SerializeField] private MoveState Crawl;

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
        
        // Execute Crawl
        if(data.input.crawling)
        {
            data.collider.SetCrawling();
            return Crawl;
        }

        // Execute Run if not moving towards collision
        if (data.input.movement > 0.0f && (data.collision & UnitCollision.Right) == 0)
        {
            return Run;
        }
        if (data.input.movement < 0.0f && (data.collision & UnitCollision.Left) == 0)
        {
            return Run;
        }

        return this;
    }
}