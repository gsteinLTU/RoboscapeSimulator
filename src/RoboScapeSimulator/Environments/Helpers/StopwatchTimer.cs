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

    public StopwatchTimer(Room room)
    {
        room.OnReset += (o, _) =>
        {
            timer.Reset();
            timer.Start();
        };

        room.OnUpdate += (o, dt) =>
        {
            if (ShowText && timer.Elapsed.Milliseconds * 10 % 10 == 0)
            {
                room.SendToClients("showText", $"Time: {timer.Elapsed.TotalSeconds:F2}", "timer", "");
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
    }
}