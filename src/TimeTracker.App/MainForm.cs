using DomainTimer = TimeTracker.Domain.Timer;

namespace TimeTracker.App;

internal sealed class MainForm : Form
{
    private readonly DomainTimer _timer;
    private readonly Label _stateLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 12F), Location = new Point(16, 16) };
    private readonly Label _dailySessionLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 28F), Location = new Point(12, 48) };

    public MainForm(DomainTimer timer)
    {
        _timer = timer;

        Text = "TimeTracker";
        ClientSize = new Size(280, 170);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var startButton = new Button { Text = "Старт", Location = new Point(16, 120), Size = new Size(120, 34) };
        var stopButton = new Button { Text = "Стоп", Location = new Point(144, 120), Size = new Size(120, 34) };
        startButton.Click += (_, _) => Run(_timer.Start);
        stopButton.Click += (_, _) => Run(_timer.Stop);
        Controls.AddRange([_stateLabel, _dailySessionLabel, startButton, stopButton]);

        var refreshTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        refreshTimer.Tick += (_, _) => ShowTimer();
        refreshTimer.Start();
        Disposed += (_, _) => refreshTimer.Dispose();

        ShowTimer();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        Run(_timer.Exit);
        base.OnFormClosing(e);
    }

    private void Run(Action timerAction)
    {
        try
        {
            timerAction();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Не удалось записать Timer Log: {ex.Message}", "TimeTracker", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        ShowTimer();
    }

    private void ShowTimer()
    {
        var dailySession = _timer.DailySession;
        _stateLabel.Text = _timer.State.ToString();
        _dailySessionLabel.Text = $"{(int)dailySession.TotalHours:00}:{dailySession.Minutes:00}:{dailySession.Seconds:00}";
    }
}
