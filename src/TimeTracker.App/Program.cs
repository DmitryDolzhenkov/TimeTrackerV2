using TimeTracker.Domain;
using DomainTimer = TimeTracker.Domain.Timer;

namespace TimeTracker.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeTracker",
            "timer-log.jsonl");

        DomainTimer timer;
        try
        {
            timer = new DomainTimer(new JsonlTimerLog(logPath), TimeProvider.System);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Не удалось прочитать Timer Log: {ex.Message}", "TimeTracker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm(timer));
    }
}
