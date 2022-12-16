using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.RobotScape;
using RoboScapeSimulator.Environments.Helpers;

namespace RoboScapeSimulator.Environments
{
    class TugOfWarEnvironment : EnvironmentConfiguration
    {
        public TugOfWarEnvironment()
        {
            Name = "Tug of War";
            ID = "tugofwar";
            Description = "Two teams compete to move one robot";
        }

        public override object Clone()
        {
            return new TugOfWarEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            StopwatchTimer sw = new(room, false);

            // Ground
            _ = new Ground(room);

            // Walls
            float wallX = 4f;
            float wallZ = 7f;

            // Outer walls
            EnvironmentUtils.MakeWalls(room, wallX, wallZ);

            // Start and team2 areas
            var team1Area = new Cube(room, wallX, 0.01f, 1, new(0, 0.005f, -wallZ / 2 + 0.5f), Quaternion.CreateFromYawPitchRoll(0, 0.05f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#D22" });
            var team1Trigger = new Trigger(room, team1Area.Position + new Vector3(0, 0, 0.3f), team1Area.Orientation, team1Area.Width, 2, team1Area.Depth);
            var team2Area = new Cube(room, wallX, 0.01f, 1, new(0, 0.005f, wallZ / 2 - 0.5f), Quaternion.CreateFromYawPitchRoll(0, -0.05f, 0), isKinematic: true, visualInfo: new VisualInfo() { Color = "#2D2" });
            var team2Trigger = new Trigger(room, team2Area.Position + new Vector3(0, 0, 0.3f), team2Area.Orientation, team2Area.Width, 2, team2Area.Depth);

            team1Trigger.OnTriggerEnter += (trigger, ent) =>
            {
                if (ent is Robot r)
                {
                    sw.Stop();
                    sw.ShowText = false;
                    room.SendToClients("showText", "Team 1 Wins!", "timer", "");
                }
            };

            team2Trigger.OnTriggerEnter += (trigger, ent) =>
            {
                if (ent is Robot r)
                {
                    sw.Stop();
                    sw.ShowText = false;
                    room.SendToClients("showText", "Team 2 Wins!", "timer", "");
                }
            };


            // Robot
            var robot = new ParallaxRobot(room, new(0, 0.15f, -wallZ / 2 + 0.5f), Quaternion.Identity);

            room.OnReset += (o, e) =>
            {
                sw.Reset();

                sw.ShowText = true;

                void handler(object? o, byte[] e)
                {
                    sw.Start();
                    robot.OnCommand -= handler;
                }

                robot.OnCommand += handler;
            };


        }
    }
}