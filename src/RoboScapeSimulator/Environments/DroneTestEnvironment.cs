using System.Diagnostics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Drones;
using RoboScapeSimulator.IoTScape.Devices;

namespace RoboScapeSimulator.Environments
{
    class DroneTestEnvironment : EnvironmentConfiguration
    {

        readonly uint _drones = 1;

        public DroneTestEnvironment(uint drones = 1)
        {

            Name = $"Drone Test With {drones} drone(s)";
            ID = $"dronetest_{drones}d";
            Description = "Drone test";
            Category = "§_Testing";
            _drones = drones;
        }

        public override object Clone()
        {
            return new DroneTestEnvironment(_drones);
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up DroneTestEnvironment environment");

            // Ground
            _ = new Ground(room);

            // Demo drones
            for (int i = 0; i < _drones; i++)
            {
                var drone = new Drone(room);
                var positionSensor = new PositionSensor(drone);
                positionSensor.Setup(room);
            }
        }
    }
}