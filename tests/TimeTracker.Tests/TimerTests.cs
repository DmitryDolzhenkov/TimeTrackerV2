using TimeTracker.Domain;
using Timer = TimeTracker.Domain.Timer;

namespace TimeTracker.Tests;

public sealed class TimerTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("timetracker-tests-").FullName;
    private readonly ManualTimeProvider _time = new(new DateTimeOffset(2026, 11, 4, 9, 0, 0, TimeSpan.FromHours(3)));
    private readonly JsonlTimerLog _log;

    public TimerTests() => _log = new JsonlTimerLog(LogPath);

    private string LogPath => Path.Combine(_directory, "timer-log.jsonl");

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private Timer Launch() => new(_log, _time);

    [Fact]
    public void A_new_launch_begins_in_stop_and_writes_nothing()
    {
        var timer = Launch();

        Assert.Equal(TimerState.Stop, timer.State);
        Assert.Empty(_log.ReadAll());
    }

    [Fact]
    public void Start_moves_stop_to_active_and_records_the_transition()
    {
        var timer = Launch();

        timer.Start();

        Assert.Equal(TimerState.Active, timer.State);
        var transition = Assert.Single(_log.ReadAll());
        Assert.Equal(TimerState.Stop, transition.OldState);
        Assert.Equal(TimerState.Active, transition.NewState);
        Assert.Equal("start", transition.Reason);
        Assert.Equal(1, transition.SchemaVersion);
        Assert.Equal(new DateTimeOffset(2026, 11, 4, 9, 0, 0, TimeSpan.FromHours(3)), transition.Timestamp);
        Assert.Equal(TimeSpan.FromHours(3), transition.Timestamp.Offset);
    }

    [Fact]
    public void Stop_moves_active_to_stop_and_records_the_transition()
    {
        var timer = Launch();
        timer.Start();

        timer.Stop();

        Assert.Equal(TimerState.Stop, timer.State);
        var transition = _log.ReadAll()[^1];
        Assert.Equal(TimerState.Active, transition.OldState);
        Assert.Equal(TimerState.Stop, transition.NewState);
        Assert.Equal("stop", transition.Reason);
    }

    [Fact]
    public void Each_transition_gets_its_own_id()
    {
        var timer = Launch();

        timer.Start();
        timer.Stop();

        var ids = _log.ReadAll().Select(t => t.Id).ToList();
        Assert.Equal(2, ids.Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, ids);
    }

    [Fact]
    public void Repeated_start_in_active_creates_no_transition()
    {
        var timer = Launch();
        timer.Start();

        timer.Start();

        Assert.Equal(TimerState.Active, timer.State);
        Assert.Single(_log.ReadAll());
    }

    [Fact]
    public void Stop_in_stop_creates_no_transition()
    {
        var timer = Launch();

        timer.Stop();

        Assert.Equal(TimerState.Stop, timer.State);
        Assert.Empty(_log.ReadAll());
    }

    [Fact]
    public void The_daily_session_grows_while_active_and_freezes_after_stop()
    {
        var timer = Launch();
        timer.Start();

        _time.Advance(TimeSpan.FromMinutes(10));
        Assert.Equal(TimeSpan.FromMinutes(10), timer.DailySession);

        timer.Stop();
        _time.Advance(TimeSpan.FromMinutes(5));
        Assert.Equal(TimeSpan.FromMinutes(10), timer.DailySession);
    }

    [Fact]
    public void A_new_launch_on_the_same_day_shows_the_previous_value_and_continues_from_it()
    {
        var first = Launch();
        first.Start();
        _time.Advance(TimeSpan.FromMinutes(25));
        first.Stop();
        _time.Advance(TimeSpan.FromMinutes(60));

        var second = Launch();
        Assert.Equal(TimerState.Stop, second.State);
        Assert.Equal(TimeSpan.FromMinutes(25), second.DailySession);

        second.Start();
        _time.Advance(TimeSpan.FromMinutes(5));
        Assert.Equal(TimeSpan.FromMinutes(30), second.DailySession);
    }

    [Fact]
    public void A_new_launch_after_an_abnormal_exit_in_active_begins_in_stop_without_counting_the_open_interval()
    {
        var previous = Launch();
        previous.Start();
        _time.Advance(TimeSpan.FromMinutes(40));

        var next = Launch();

        Assert.Equal(TimerState.Stop, next.State);
        Assert.Equal(TimeSpan.Zero, next.DailySession);
        Assert.Single(_log.ReadAll());
    }

    [Fact]
    public void A_torn_last_line_does_not_prevent_launch_or_further_recording()
    {
        var first = Launch();
        first.Start();
        _time.Advance(TimeSpan.FromMinutes(15));
        first.Stop();
        File.AppendAllText(LogPath, "{\"id\":\"torn");

        var second = Launch();
        Assert.Equal(TimeSpan.FromMinutes(15), second.DailySession);
        second.Start();
        _time.Advance(TimeSpan.FromMinutes(5));
        second.Stop();

        Assert.Equal(TimeSpan.FromMinutes(20), Launch().DailySession);
    }

    [Fact]
    public void Exit_in_active_records_a_transition_to_stop_with_the_exit_reason()
    {
        var timer = Launch();
        timer.Start();
        _time.Advance(TimeSpan.FromMinutes(5));

        timer.Exit();

        Assert.Equal(TimerState.Stop, timer.State);
        var transition = _log.ReadAll()[^1];
        Assert.Equal(TimerState.Active, transition.OldState);
        Assert.Equal(TimerState.Stop, transition.NewState);
        Assert.Equal("exit", transition.Reason);
        Assert.Equal(TimeSpan.FromMinutes(5), Launch().DailySession);
    }

    [Fact]
    public void Exit_in_stop_creates_no_transition()
    {
        Launch().Exit();

        Assert.Empty(_log.ReadAll());
    }

    [Fact]
    public void A_failed_write_leaves_the_timer_state_unchanged()
    {
        var timer = Launch();

        using (new FileStream(LogPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            Assert.ThrowsAny<IOException>(timer.Start);
        }

        Assert.Equal(TimerState.Stop, timer.State);
        Assert.Empty(_log.ReadAll());
    }

    private sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override TimeZoneInfo LocalTimeZone { get; } =
            TimeZoneInfo.CreateCustomTimeZone("test", start.Offset, "test", "test");

        public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();

        public void Advance(TimeSpan delta) => _now += delta;
    }
}
