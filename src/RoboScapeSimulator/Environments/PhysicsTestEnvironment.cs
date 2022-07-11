using System.Diagnostics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class PhysicsTestEnvironment : EnvironmentConfiguration
    {
        public PhysicsTestEnvironment()
        {
            Name = "PhysicsTestEnvironment";
            ID = "phystest";
            Description = "Physics demo environment";
            Category = "§_Testing";
        }

        PositionSensor? locationSensor;

        public override object Clone()
        {
            return new PhysicsTestEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up PhysicsTestEnvironment environment");

            room.SimInstance.IntegratorCallbacks.LinearDamping = 0;

            // Ground
            _ = new Ground(room);

            // Walls
            EnvironmentUtils.MakeWalls(room);

            var cube = new Cube(room, visualInfo: new VisualInfo() { Color = "#B85" });

            PhysicsService physicsService = new(cube);
            physicsService.Setup(room);

            locationSensor = new(cube.BodyReference, "")
            {
                IDOverride = physicsService.ID
            };

            locationSensor.Setup(room);
        }
    }
}