using UnityEngine;

[CreateAssetMenu(fileName = "Dive", menuName = "States/Movement/Dive", order = 0)]
public class DiveState : MoveState
{
    [SerializeField] private MoveState Idle;
    [SerializeField] private MoveState Slide;
    [SerializeField] private MoveState Crawl;
    [SerializeField] private MoveState Jump;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        animator.Play("Dive");
        data.velocity *= data.stats.diveVelocityMultiplier;
        data.t = 0;
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        if ((data.collision & UnitCollision.Ground) != 0)
        {
            if (data.input.crawling == false || data.t != 0.0f)
            {

                // Set unit timer to exit animation duration
                if (data.t == 0)
                {
                    // Execute animation transition
                    animator.Play("DiveFlip");
                    // Update animator to transition to relevant state
                    animator.Update(0);
                    animator.Update(0);
                    
                    data.t = animator.GetCurrentAnimatorStateInfo(0).length;
                    data.collider.SetStanding();
                }
                // Tick unit timer
                if(data.t != 0.0f)
                {
                    data.t = Mathf.Max(0.0f, data.t - Time.deltaTime);

                    // Execute Jump
                    if (data.t < 0.1f && data.ShouldJump())
                    {
                        return Jump;
                    }
                    
                    if(data.t == 0.0f)
                        return Idle;
                }
            }
            else
            {
                if(data.velocity.magnitude > data.stats.walkSpeed)
                {
                    // Execute Slide
                    animator.Play("BellySlide");
                    return Slide;
                }
                else
                {
                    // Execute Crawl
                    animator.Play("Crawl");
                    return Crawl;
                }
            }
        }

        return this;
    }

}
