using System.Windows.Threading;

namespace GameOcrOverlay.Services;

public sealed class DispatcherTimerAdapter
{
    private readonly DispatcherTimer _timer = new();

    public DispatcherTimerAdapter()
    {
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Elapsed?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Elapsed;

    public void Restart(TimeSpan interval)
    {
        _timer.Stop();
        _timer.Interval = interval;
        _timer.Start();
    }
}
