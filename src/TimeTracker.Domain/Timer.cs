namespace TimeTracker.Domain;

/// <summary>The single Timer. Every launch begins in Stop; the Daily Session is derived from the Timer Log.</summary>
public sealed class Timer
{
    private const int SchemaVersion = 1;

    private readonly JsonlTimerLog _log;
    private readonly TimeProvider _time;
    private readonly List<Transition> _transitions;

    public Timer(JsonlTimerLog log, TimeProvider time)
    {
        _log = log;
        _time = time;
        _transitions = [.. log.ReadAll()];
    }

    public TimerState State { get; private set; } = TimerState.Stop;

    public TimeSpan DailySession => Domain.DailySession.Calculate(_transitions, _time.GetLocalNow(), State);

    public void Start()
    {
        if (State == TimerState.Stop)
        {
            MoveTo(TimerState.Active, "start");
        }
    }

    public void Stop() => MoveToStop("stop");

    /// <summary>Records the end of counting when the application exits.</summary>
    public void Exit() => MoveToStop("exit");

    private void MoveToStop(string reason)
    {
        if (State == TimerState.Active)
        {
            MoveTo(TimerState.Stop, reason);
        }
    }

    // The log is written first: if the write fails, the Timer State does not change.
    private void MoveTo(TimerState newState, string reason)
    {
        var transition = new Transition(Guid.NewGuid(), SchemaVersion, _time.GetLocalNow(), State, newState, reason);
        _log.Append(transition);
        _transitions.Add(transition);
        State = newState;
    }
}
