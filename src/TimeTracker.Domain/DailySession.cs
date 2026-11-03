namespace TimeTracker.Domain;

public static class DailySession
{
    /// <summary>Calculates the Daily Session of the local day of <paramref name="now"/> from the Timer Log.</summary>
    public static TimeSpan Calculate(IReadOnlyList<Transition> transitions, DateTimeOffset now, TimerState timerState)
    {
        var total = TimeSpan.Zero;

        for (var i = 0; i < transitions.Count; i++)
        {
            var activation = transitions[i];
            if (activation.NewState != TimerState.Active || LocalDay(activation.Timestamp) != LocalDay(now))
            {
                continue;
            }

            var end = EndOfActiveInterval(transitions, i, now, timerState);
            if (end is { } endTime && endTime > activation.Timestamp)
            {
                total += endTime - activation.Timestamp;
            }
        }

        return total;
    }

    // Null when the application ended without closing the interval, so its end is unknown.
    private static DateTimeOffset? EndOfActiveInterval(
        IReadOnlyList<Transition> transitions, int index, DateTimeOffset now, TimerState timerState)
    {
        if (index == transitions.Count - 1)
        {
            return timerState == TimerState.Active ? now : null;
        }

        var next = transitions[index + 1];
        return next.OldState == TimerState.Active ? next.Timestamp : null;
    }

    private static DateOnly LocalDay(DateTimeOffset timestamp) => DateOnly.FromDateTime(timestamp.DateTime);
}
