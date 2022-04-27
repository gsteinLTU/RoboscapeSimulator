using System.Diagnostics;

namespace RoboScapeSimulator.Environments.Helpers;

/// <summary>
/// Adds a timer to a room with optional text shown on the client
/// </summary>
internal class StopwatchTimer
{
    public readonly StopwatchLite timer = new();

    public bool ShowText = true;

    bool shouldRun = true;

    long? lastSend = null;

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