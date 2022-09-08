using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Environments.Helpers;
using RoboScapeSimulator.IoTScape.Devices;
using RoboScapeSimulator.Physics.Bepu;
using RoboScapeSimulator.Physics.Null;

namespace RoboScapeSimulator.Environments
{
    class PhysicsTestEnvironment : EnvironmentConfiguration
    {
        
        public PhysicsTestEnvironment()
        {
            PreferredSimulationInstanceType = typeof(NullSimulationInstance);
            Name = "PhysicsTestEnvironment";
            ID = "phystest";
            Description = "Physics demo environment";
            Category = "§_Testing";
        }

        public override object Clone()
        {
            return new PhysicsTestEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up PhysicsTestEnvironment environment");

            if(room.SimInstance is BepuSimulationInstance bSim){ 
                bSim.IntegratorCallbacks.LinearDamping = 0;
            }

            // Ground
            _ = new Ground(room, 100, 100);

            // Walls
            EnvironmentUtils.MakeWalls(room, 100, 100);

            var cube = new Cube(room, initialPosition: new Vector3(0f, 0f, 1f), initialOrientation: Quaternion.Identity, visualInfo: new VisualInfo() { Color = "#B85" });
            var cube2 = new Cube(room, 0.8f, 0.675f, 0.8f, initialPosition: new Vector3(0f, 0f, 0f), initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI), visualInfo: new VisualInfo() { ModelName = "car1_blue.gltf", ModelScale = 5f });

            PhysicsService physicsService = new(cube);
            physicsService.Setup(room);
            PhysicsService physicsService2 = new(cube2);
            physicsService2.Setup(room);
        }
    }
}