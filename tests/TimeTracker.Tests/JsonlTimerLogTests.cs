using System.Text.Json;
using TimeTracker.Domain;

namespace TimeTracker.Tests;

public sealed class JsonlTimerLogTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("timetracker-tests-").FullName;
    private string LogPath => Path.Combine(_directory, "nested", "timer-log.jsonl");

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private static Transition SampleTransition(string reason = "start") => new(
        Guid.Parse("11111111-2222-3333-4444-555555555555"),
        1,
        new DateTimeOffset(2026, 11, 4, 13, 0, 0, TimeSpan.FromHours(3)),
        TimerState.Stop,
        TimerState.Active,
        reason);

    [Fact]
    public void Reading_a_missing_file_gives_an_empty_log()
    {
        Assert.Empty(new JsonlTimerLog(LogPath).ReadAll());
    }

    [Fact]
    public void An_appended_transition_is_read_back_unchanged()
    {
        var log = new JsonlTimerLog(LogPath);

        log.Append(SampleTransition());

        var read = Assert.Single(new JsonlTimerLog(LogPath).ReadAll());
        Assert.Equal(SampleTransition(), read);
        Assert.Equal(TimeSpan.FromHours(3), read.Timestamp.Offset);
    }

    [Fact]
    public void Each_entry_is_one_json_line_with_every_required_field()
    {
        new JsonlTimerLog(LogPath).Append(SampleTransition());

        var lines = File.ReadAllLines(LogPath);
        var entry = JsonDocument.Parse(Assert.Single(lines)).RootElement;
        Assert.Equal("11111111-2222-3333-4444-555555555555", entry.GetProperty("id").GetString());
        Assert.Equal(1, entry.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("2026-11-04T13:00:00+03:00", entry.GetProperty("timestamp").GetString());
        Assert.Equal("Stop", entry.GetProperty("oldState").GetString());
        Assert.Equal("Active", entry.GetProperty("newState").GetString());
        Assert.Equal("start", entry.GetProperty("reason").GetString());
    }

    [Fact]
    public void Appending_keeps_earlier_entries_in_order()
    {
        var log = new JsonlTimerLog(LogPath);

        log.Append(SampleTransition("first"));
        log.Append(SampleTransition("second"));

        Assert.Equal(["first", "second"], log.ReadAll().Select(t => t.Reason));
    }

    [Theory]
    [InlineData("{\"id\":\"11111111-22")]
    [InlineData("{}")]
    [InlineData("null")]
    public void A_torn_last_line_is_skipped(string tornLine)
    {
        var log = new JsonlTimerLog(LogPath);
        log.Append(SampleTransition("intact"));
        File.AppendAllText(LogPath, tornLine);

        Assert.Equal(["intact"], log.ReadAll().Select(t => t.Reason));
    }

    [Fact]
    public void Appending_after_a_torn_last_line_keeps_the_new_entry_readable_on_the_next_launch()
    {
        var log = new JsonlTimerLog(LogPath);
        log.Append(SampleTransition("before"));
        File.AppendAllText(LogPath, "{\"id\":\"11111111-22");

        log.Append(SampleTransition("after"));

        Assert.Equal(["before", "after"], new JsonlTimerLog(LogPath).ReadAll().Select(t => t.Reason));
    }
}
