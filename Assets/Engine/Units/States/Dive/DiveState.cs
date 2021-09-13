using UnityEngine;

[CreateAssetMenu(fileName = "Dive", menuName = "States/Movement/Dive", order = 0)]
public class DiveState : MoveState
{
    [SerializeField] private MoveState Idle;
    [SerializeField] private MoveState Slide;
    [SerializeField] private MoveState Crawl;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        animator.Play("Dive");
        data.velocity += data.velocity * data.stats.diveVelocityMultiplier;
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        if ((data.collision & UnitCollision.Ground) != 0)
        {
            if (data.input.crawling == false)
            {
                // Execute Idle
                animator.Play("Dive -> Idle");
                // Update animator to transition to relevant state
                animator.Update(0);
                animator.Update(0);
                data.lockStateDelay = animator.GetCurrentAnimatorStateInfo(0).length;
                data.collider.SetStanding();
                return Idle;
            }
            else
            {
                if(data.velocity.magnitude > data.stats.walkSpeed)
                {
                    // Execute Slide
                    animator.Play("Dive -> Slide");
                    return Slide;
                }
                else
                {
                    // Execute Crawl
                    animator.Play("Dive -> Crawl");
                    return Crawl;
                }
            }
        }

        return this;
    }

}
