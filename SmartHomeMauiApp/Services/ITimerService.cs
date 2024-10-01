namespace SmartHomeMauiApp.Services;

public interface ITimerService
{
    void Start(Action callback, int interval);
    void Stop();
}

