using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.RobotScape;

namespace RoboScapeSimulator.Environments
{
    class TriggerTestEnvironment : EnvironmentConfiguration
    {
        public TriggerTestEnvironment()
        {
            Name = "Trigger test";
            ID = "triggertest";
            Description = "Test of trigger entity";
            Category = "§_Testing";
        }

        public override object Clone()
        {
            return new TriggerTestEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up trigger test environment");

            // Ground
            _ = new Ground(room);

            // Trigger volume
            var trigger = new Trigger(room, new(0, 0.5f, 2), Quaternion.Identity, 2, 1, 2, false, true);
            trigger.OnTriggerEnter += (o, e) => { Trace.WriteLine(e.Name + " entered trigger"); };
            trigger.OnTriggerStay += (o, e) => { Trace.WriteLine(e.Name + " in trigger"); };
            trigger.OnTriggerExit += (o, e) => { Trace.WriteLine(e.Name + " exited trigger"); };
            trigger.OnTriggerEmpty += (o, e) => { Trace.WriteLine("Trigger empty"); };

            // Demo robot
            _ = new ParallaxRobot(room, new(0, 0.25f, 0), Quaternion.Identity);
        }
    }
}