using UnityEngine;

[CreateAssetMenu(fileName = "Slide", menuName = "States/Movement/Slide", order = 0)]
public class SlideState : MoveState
{
    [SerializeField] private MoveState Crawl;
    [SerializeField] private MoveState Idle;
    [SerializeField] private MoveState Jump;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        data.t = 0.0f;
        if (data.lastStateName != "Dive")
        {
            data.velocity *= data.stats.slideVelocityMultiplier;
        }
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        if (data.input.crawling == false || data.t != 0.0f)
        {

            // Set unit timer to exit animation duration
            if (data.t == 0)
            {
                // Execute animation transition
                animator.Play(data.lastStateName == "Dive" ? "DiveFlip" : "SlideExit");
                // Update animator to transition to relevant state
                animator.Update(0);
                animator.Update(0);

                data.t = animator.GetCurrentAnimatorStateInfo(0).length;
                data.collider.SetStanding();
            }
            // Tick unit timer
            if (data.t != 0.0f)
            {
                data.t = Mathf.Max(0.0f, data.t - Time.deltaTime);

                // Execute Jump
                if (data.t < 0.1f && data.ShouldJump())
                {
                    return Jump;
                }

                if (data.t == 0.0f)
                    return Idle;
            }
        }
        
        // Execute Crawl when speed drops below walking
        if (data.input.crawling && Mathf.Abs(data.velocity.x) <= data.stats.walkSpeed)
        {
            animator.Play("Crawl");
            return Crawl;
        }

        return this;
    }

}
