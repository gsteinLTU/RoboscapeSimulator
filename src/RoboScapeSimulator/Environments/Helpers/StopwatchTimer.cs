using System.Diagnostics;

namespace RoboScapeSimulator.Environments.Helpers;

internal class StopwatchTimer
{
    readonly Stopwatch sw = new();

    public bool ShowText = true;

    public StopwatchTimer(Room room)
    {
        room.OnReset += (o, _) =>
        {
            sw.Reset();
            sw.Start();
        };

        room.OnUpdate += (o, dt) =>
        {
            if (ShowText && sw.Elapsed.Milliseconds * 10 % 10 == 0)
            {
                room.SendToClients("showText", $"Time: {sw.Elapsed.TotalSeconds:F2}", "timer", "");
            }
        };
    }

    public void Stop()
    {
        sw.Stop();
    }

    public void Start()
    {
        sw.Start();
    }

    public void Reset()
    {
        sw.Stop();
    }
}