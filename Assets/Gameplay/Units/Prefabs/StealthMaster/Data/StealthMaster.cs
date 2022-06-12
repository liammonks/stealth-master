using States;
using SM = States.StealthMaster;

public class StealthMaster : StateMachine
{
    protected override void Start()
    {
        base.Start();
        states.Add(UnitState.Idle, new SM.Idle(unit));
        states.Add(UnitState.Run, new SM.Run(unit));
        states.Add(UnitState.Jump, new SM.Jump(unit));
        states.Add(UnitState.Fall, new Fall(unit));
        states.Add(UnitState.CrawlIdle, new SM.CrawlIdle(unit));
        states.Add(UnitState.Crawl, new Crawl(unit));
        states.Add(UnitState.Dive, new SM.Dive(unit));
        states.Add(UnitState.Slide, new Slide(unit));
        //states.Add(UnitState.WallSlide, new WallSlide(data));
        //states.Add(UnitState.LedgeGrab, new LedgeGrab(data));
        //states.Add(UnitState.WallJump, new WallJump(data));
        //states.Add(UnitState.Climb, new States.Climb(data));
        //states.Add(UnitState.VaultOver, new States.VaultOver(data));
        //states.Add(UnitState.VaultOn, new States.VaultOn(data));
        //states.Add(UnitState.Melee, new States.Melee(data));
        //states.Add(UnitState.JumpMelee, new States.JumpMelee(data));
    }
}
