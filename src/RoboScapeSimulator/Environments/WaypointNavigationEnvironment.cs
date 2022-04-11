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
            Random rng = new();

            // Ground
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // robot
            _ = new ParallaxRobot(room, rng.PointOnCircle(0.5f, 0.25f), debug: false);

            var waypoint = new Trigger(room, rng.PointOnCircle(1.5f), Quaternion.Identity, 0.2f, 0.1f, 0.2f);
            var waypoint_X_1 = new VisualOnlyEntity(room, initialPosition: waypoint.BodyReference.Pose.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, 45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });
            var waypoint_X_2 = new VisualOnlyEntity(room, initialPosition: waypoint.BodyReference.Pose.Position, initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, -45), width: 0.1f, height: 0.05f, depth: 0.5f, visualInfo: new VisualInfo() { Color = "#633" });

            waypoint.OnTriggerEnter += (o, e) =>
            {
                // Move waypoint
                waypoint.BodyReference.Pose.Position = rng.PointOnCircle(1.5f);
                waypoint_X_1.Position = waypoint.BodyReference.Pose.Position;
                waypoint_X_2.Position = waypoint.BodyReference.Pose.Position;
            };
        }
    }
}