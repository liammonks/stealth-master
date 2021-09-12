using UnityEngine;

[CreateAssetMenu(fileName = "Crawl", menuName = "States/Movement/Crawl", order = 0)]
public class CrawlState : MoveState
{
    [SerializeField] private MoveState Idle;

    public override MoveState Initialise(UnitData data, Animator animator)
    {
        data.collider.SetCrawling();
        animator.Play("Crawl");
        return this;
    }
    
    public override MoveState Execute(UnitData data, Animator animator)
    {
        // Apply movement input
        if (Mathf.Abs(data.velocity.x) < data.stats.walkSpeed)
        {
            data.velocity.x += data.stats.walkSpeed * data.input.movement * Time.deltaTime * data.stats.groundAuthority;
            data.velocity.x = Mathf.Clamp(data.velocity.x, -data.stats.walkSpeed, data.stats.walkSpeed);
        }
        
        // Return to Idle
        if(!data.input.crawlQueued && (data.collision & UnitCollision.Ground) != 0)
        {
            data.collider.SetStanding();
            return Idle;
        }
        
        return this;
    }

}
