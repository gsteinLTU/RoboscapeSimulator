namespace RoboScapeSimulator.Environments.Helpers;

/// <summary>
/// Adds a timer to a room with optional text shown on the client
/// </summary>
internal class StopwatchTimer
{
    public readonly StopwatchLite timer = new();

    public bool ShowText = true;

    bool shouldRun = false;

    long? lastSend = null;

    public bool IsRunning { get => timer.IsRunning; }

    public StopwatchTimer(Room room, bool autoSetup = true)
    {
        room.OnUpdate += (o, dt) =>
        {
            if (ShowText && (timer.ElapsedMillis - (lastSend ?? -1000)) >= 120)
            {
                room.SendToClients("showText", $"Time: {timer.ElapsedSeconds:F2}", "timer", "");
                lastSend = timer.ElapsedMillis;
            }
        };

        room.OnHibernateStart += (o, e) =>
        {
            shouldRun = timer.IsRunning;
            timer.Stop();
        };

        room.OnHibernateEnd += (o, e) =>
        {
            if (shouldRun)
            {
                timer.Start();
            }
        };

        shouldRun = autoSetup;

        if (autoSetup)
        {
            room.OnReset += (o, _) =>
            {
                Reset();
                Start();
            };

            Start();
        }
    }

    public void Stop()
    {
        timer.Stop();
        lastSend = null;
    }

    public void Start()
    {
        timer.Start();
        lastSend = null;
    }

    public void Reset()
    {
        timer.Stop();
        timer.Reset();
        lastSend = null;

        if (shouldRun)
        {
            timer.Start();
        }
    }
}