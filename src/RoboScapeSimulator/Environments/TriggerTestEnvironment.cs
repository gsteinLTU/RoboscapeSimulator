using System.Diagnostics;
using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;

namespace RoboScapeSimulator.Environments
{
    class TriggerTestEnvironment : EnvironmentConfiguration
    {
        public TriggerTestEnvironment()
        {
            Name = "Trigger test";
            ID = "triggertest";
            Description = "Test of trigger entity";
        }

        public override object Clone()
        {
            return new TriggerTestEnvironment();
        }

        public override void Setup(Room room)
        {
            Trace.WriteLine("Setting up trigger test environment");

            // Ground
            _ = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

            // Trigger volume
            var trigger = new Trigger(room, new(0, 0.5f, 2), Quaternion.Identity, 2, 1, 2, true);
            trigger.OnTriggerEnter += (o, e) => { Console.WriteLine(e.Name + " entered trigger"); };
            trigger.OnTriggerStay += (o, e) => { Console.WriteLine(e.Name + " in trigger"); };
            trigger.OnTriggerExit += (o, e) => { Console.WriteLine(e.Name + " exited trigger"); };
            trigger.OnTriggerEmpty += (o, e) => { Console.WriteLine("Trigger empty"); };

            // Demo robot
            _ = new ParallaxRobot(room, new(0, 0.25f, 0), Quaternion.Identity);
        }
    }
}