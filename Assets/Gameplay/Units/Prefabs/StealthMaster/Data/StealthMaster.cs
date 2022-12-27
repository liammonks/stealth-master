using States;
using SM = States.StealthMaster;

public class StealthMaster : StateMachine
{
    protected override void CollectStates()
    {
        states.Add(UnitState.Idle,          new SM.Idle(unit));
        states.Add(UnitState.Run,           new SM.Run(unit));
        states.Add(UnitState.Jump,          new SM.Jump(unit));
        states.Add(UnitState.Fall,          new SM.Fall(unit));
        states.Add(UnitState.CrawlIdle,     new SM.CrawlIdle(unit));
        states.Add(UnitState.Crawl,         new Crawl(unit));
        states.Add(UnitState.Dive,          new SM.Dive(unit));
        states.Add(UnitState.Slide,         new SM.Slide(unit));
        states.Add(UnitState.WallSlide,     new SM.WallSlide(unit));
        states.Add(UnitState.LedgeGrab,     new SM.LedgeGrab(unit));
        states.Add(UnitState.WallJump,      new SM.WallJump(unit));
        states.Add(UnitState.Climb,         new Climb(unit));
        states.Add(UnitState.VaultOver,     new VaultOver(unit));
        states.Add(UnitState.VaultOn,       new VaultOn(unit));
        //states.Add(UnitState.Melee, new States.Melee(unit));
        //states.Add(UnitState.JumpMelee, new States.JumpMelee(unit));
    }
}
