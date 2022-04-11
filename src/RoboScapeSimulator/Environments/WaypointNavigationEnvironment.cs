using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
namespace RoboScapeSimulator.Environments
{
    class WaypointNavigationEnvironment : EnvironmentConfiguration
    {
        public WaypointNavigationEnvironment()
        {
            Name = "Waypoint Navigation";
            ID = "WaypointNavigation";
            Description = "Robot must find waypoints";
        }

        public override object Clone()
        {
            return new WaypointNavigationEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine($"Setting up {Name} environment");

            // Ground
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // robot
            _ = new ParallaxRobot(room, debug: false);

            Random rng = new();
            var waypoint = new Trigger(room, rng.PointOnCircle(2), Quaternion.Identity);
            var waypoint_X_1 = new Cube(room, initialPosition: waypoint.BodyReference.Pose.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, 45));
            var waypoint_X_2 = new Cube(room, initialPosition: waypoint.BodyReference.Pose.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45));

        }
    }
}