using TimeTracker.Domain;

namespace TimeTracker.Tests;

public sealed class DailySessionTests
{
    private static readonly TimeSpan Moscow = TimeSpan.FromHours(3);

    private static DateTimeOffset At(int day, int hour, int minute = 0, TimeSpan? offset = null) =>
        new(2026, 11, day, hour, minute, 0, offset ?? Moscow);

    private static Transition ToActive(DateTimeOffset at) => new(Guid.NewGuid(), 1, at, TimerState.Stop, TimerState.Active, "start");

    private static Transition ToStop(DateTimeOffset at) => new(Guid.NewGuid(), 1, at, TimerState.Active, TimerState.Stop, "stop");

    [Fact]
    public void An_empty_log_gives_zero()
    {
        Assert.Equal(TimeSpan.Zero, DailySession.Calculate([], At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void A_closed_active_interval_counts_its_length()
    {
        Transition[] log = [ToActive(At(4, 9)), ToStop(At(4, 9, 30))];

        Assert.Equal(TimeSpan.FromMinutes(30), DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void Several_intervals_in_one_day_add_up()
    {
        Transition[] log =
        [
            ToActive(At(4, 9)), ToStop(At(4, 10)),
            ToActive(At(4, 11)), ToStop(At(4, 11, 15)),
        ];

        Assert.Equal(TimeSpan.FromMinutes(75), DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void An_active_interval_in_progress_counts_up_to_now()
    {
        Transition[] log = [ToActive(At(4, 11, 50))];

        Assert.Equal(TimeSpan.FromMinutes(10), DailySession.Calculate(log, At(4, 12), TimerState.Active));
    }

    [Fact]
    public void A_negative_interval_between_entries_counts_as_zero()
    {
        Transition[] log =
        [
            ToActive(At(4, 10)), ToStop(At(4, 9)),
            ToActive(At(4, 11)), ToStop(At(4, 11, 20)),
        ];

        Assert.Equal(TimeSpan.FromMinutes(20), DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void An_active_interval_in_progress_with_the_clock_moved_back_counts_as_zero()
    {
        Transition[] log = [ToActive(At(4, 12))];

        Assert.Equal(TimeSpan.Zero, DailySession.Calculate(log, At(4, 11), TimerState.Active));
    }

    [Fact]
    public void Intervals_from_another_local_day_are_not_counted()
    {
        Transition[] log = [ToActive(At(3, 9)), ToStop(At(3, 17))];

        Assert.Equal(TimeSpan.Zero, DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void The_local_day_of_an_entry_comes_from_its_recorded_offset()
    {
        // 23:30 at UTC-5 on Nov 3 is already 07:30 on Nov 4 in Moscow, but it belongs to Nov 3.
        var offset = TimeSpan.FromHours(-5);
        Transition[] log = [ToActive(At(3, 23, 30, offset)), ToStop(At(3, 23, 45, offset))];

        Assert.Equal(TimeSpan.Zero, DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void An_interval_left_open_by_an_abnormal_exit_counts_as_zero_when_the_timer_is_stopped()
    {
        Transition[] log = [ToActive(At(4, 9))];

        Assert.Equal(TimeSpan.Zero, DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }

    [Fact]
    public void An_interval_left_open_by_an_abnormal_exit_counts_as_zero_after_the_next_start()
    {
        Transition[] log = [ToActive(At(4, 9)), ToActive(At(4, 11)), ToStop(At(4, 11, 10))];

        Assert.Equal(TimeSpan.FromMinutes(10), DailySession.Calculate(log, At(4, 12), TimerState.Stop));
    }
}
