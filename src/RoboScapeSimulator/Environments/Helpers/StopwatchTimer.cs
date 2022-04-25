using System.Diagnostics;

namespace RoboScapeSimulator.Environments.Helpers;

/// <summary>
/// Adds a timer to a room with optional text shown on the client
/// </summary>
internal class StopwatchTimer
{
    public readonly Stopwatch timer = new();

    public bool ShowText = true;

    bool shouldRun = true;

    TimeSpan? lastSend = null;

    public StopwatchTimer(Room room)
    {
        room.OnReset += (o, _) =>
        {
            timer.Reset();
            timer.Start();
            lastSend = null;
        };

        room.OnUpdate += (o, dt) =>
        {
            if (ShowText && (timer.Elapsed - (lastSend ?? TimeSpan.FromMilliseconds(-1000))).TotalMilliseconds >= 120)
            {
                room.SendToClients("showText", $"Time: {timer.Elapsed.TotalSeconds:F2}", "timer", "");
                lastSend = timer.Elapsed;
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

        timer.Start();
    }

    public void Stop()
    {
        timer.Stop();
    }

    public void Start()
    {
        timer.Start();
    }

    public void Reset()
    {
        timer.Stop();
        lastSend = null;
    }
}