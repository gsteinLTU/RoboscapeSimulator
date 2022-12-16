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
        readonly bool _extraObjects = false;

        public PhysicsTestEnvironment(bool extraObjects = false)
        {
            _extraObjects = extraObjects;
            PreferredSimulationInstanceType = typeof(NullSimulationInstance);
            Name = "Physics Environment"  + (extraObjects ? " with extra objects" : "");
            ID = "phystest" + (extraObjects ? "_extra" : "");
            Description = "Physics demo environment";
            Category = "§_Testing";
        }

        public override object Clone()
        {
            return new PhysicsTestEnvironment(_extraObjects);
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

            PhysicsService physicsService = new(cube, room.Name + "_cube");
            physicsService.Setup(room);
            PhysicsService physicsService2 = new(cube2, room.Name + "_robot");
            physicsService2.Setup(room);

            if(_extraObjects){
                var cube3 = new Cube(room, 0.8f, 0.675f, 0.8f, initialPosition: new Vector3(1f, 0f, 0f), initialOrientation: Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI), visualInfo: new VisualInfo() { ModelName = "car1_green.gltf", ModelScale = 5f });
                var cube4 = new Cube(room, depth: 2, initialPosition: new Vector3(0f, 0f, 4f), initialOrientation: Quaternion.Identity, visualInfo: new VisualInfo() { Color = "#58b" });

                PhysicsService physicsService3 = new(cube3, room.Name + "_robot2");
                physicsService3.Setup(room);
                PhysicsService physicsService4 = new(cube4, room.Name + "_cube2");
                physicsService4.Setup(room);
            }
        }
    }
}