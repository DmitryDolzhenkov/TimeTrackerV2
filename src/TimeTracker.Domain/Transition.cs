namespace TimeTracker.Domain;

/// <summary>A Timer Log entry recording a change from one Timer State to another.</summary>
public sealed record Transition(
    Guid Id,
    int SchemaVersion,
    DateTimeOffset Timestamp,
    TimerState OldState,
    TimerState NewState,
    string Reason);
