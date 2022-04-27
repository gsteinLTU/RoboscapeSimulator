namespace RoboScapeSimulator.Environments.Helpers;

/// <summary>
/// Uses Environment.TickCount64 instead of System.Diagnostics.Stopwatch to provide a lighter stopwatch interface
/// For higher precision, use Stopwatch, for most RoboScape Online tasks (e.g. timing that a robot waits at a waypoint for 3 seconds), this is sufficient
/// </summary>
public class StopwatchLite
{
    long startTicks;

    long extra = 0;

    bool isRunning = false;

    public StopwatchLite()
    {
        startTicks = Environment.TickCount64;
    }

    public long ElapsedMillis { get => isRunning ? (Environment.TickCount64 - startTicks + extra) : extra; }
    public float ElapsedSeconds { get => ElapsedMillis / 1000.0f; }
    public TimeSpan Elapsed { get => new(ElapsedMillis * 10); }
    public bool IsRunning { get => isRunning; }

    public void Start()
    {
        startTicks = Environment.TickCount64;
        isRunning = true;
    }
    public void Restart()
    {
        Reset();
        Start();
    }

    public void Stop()
    {
        extra = ElapsedMillis;
        isRunning = false;
    }

    public void Reset()
    {
        isRunning = false;
        extra = 0;
    }
}