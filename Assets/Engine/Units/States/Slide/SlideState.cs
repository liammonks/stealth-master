using UnityEngine;

[CreateAssetMenu(fileName = "Slide", menuName = "States/Movement/Slide", order = 0)]
public class SlideState : MoveState
{
    [SerializeField] private MoveState Crawl;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        // Execute Crawl when speed drops below walking
        if (data.velocity.magnitude <= data.stats.walkSpeed)
        {
            animator.Play("Slide -> Crawl");
            return Crawl;
        }

        return this;
    }

}
