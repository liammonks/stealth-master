using States.StealthMaster;

public class StealthMaster : StateMachine
{
    protected override void Awake()
    {
        base.Awake();
        states.Add(UnitState.Idle,      new Idle(data));
        states.Add(UnitState.Run,       new Run(data));
        states.Add(UnitState.Jump,      new Jump(data));
        states.Add(UnitState.Fall,      new Fall(data));
        states.Add(UnitState.CrawlIdle, new CrawlIdle(data));
        states.Add(UnitState.Crawl,     new States.Crawl(data));
        states.Add(UnitState.Dive,      new Dive(data));
        states.Add(UnitState.Slide,     new Slide(data));
        states.Add(UnitState.WallSlide, new WallSlide(data));
        states.Add(UnitState.LedgeGrab, new LedgeGrab(data));
        states.Add(UnitState.WallJump,  new WallJump(data));
        states.Add(UnitState.Climb,     new States.Climb(data));
        states.Add(UnitState.VaultOver, new States.VaultOver(data));
        states.Add(UnitState.VaultOn, new States.VaultOn(data));
    }
}
