using States;

public class StealthMaster : StateMachine
{
    protected override void Awake()
    {
        base.Awake();
        states.Add(UnitState.Idle, new StealthMaster_Idle(data));
        states.Add(UnitState.Run, new Run(data));
        states.Add(UnitState.Jump, new Jump(data));
        states.Add(UnitState.Fall, new Fall(data));
        states.Add(UnitState.CrawlIdle, new CrawlIdle(data));
    }
}
