namespace SmartHomeMauiApp.Services;

public class TimerService : ITimerService
{
    private System.Timers.Timer? _timer;

    public void Start(Action callback, int interval)
    {
        _timer = new System.Timers.Timer(interval);
        _timer.Elapsed += (sender, args) => callback();
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}

