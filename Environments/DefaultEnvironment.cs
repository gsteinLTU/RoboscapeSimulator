using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

class DefaultEnvironment : EnvironmentConfiguration
{
    public DefaultEnvironment()
    {
        Name = "Default";
        ID = "default";
        Description = "The default environment";
    }

    public override void Setup(Room room)
    {
        Console.WriteLine("Setting up default environment");

        // Ground
        var groundHandle = room.SimInstance.Simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 0), new CollidableDescription(room.SimInstance.Simulation.Shapes.Add(new Box(200, 1, 200)), 0.1f)));
        room.SimInstance.NamedStatics.Add("ground", room.SimInstance.Simulation.Statics.GetStaticReference(groundHandle));

        // Demo robot
        var robot = new ParallaxRobot(room);
        room.SimInstance.NamedBodies.Add("robot_" + Robot.BytesToHexstring(robot.MacAddress, ""), robot.MainBodyReference);
        // NamedBodies.Add("wheelL", Simulation.Bodies.GetBodyReference(robot.LWheel));
        // NamedBodies.Add("wheelR", Simulation.Bodies.GetBodyReference(robot.RWheel));
        // NamedBodies.Add("wheelRear", Simulation.Bodies.GetBodyReference(robot.RearWheel));

        room.SimInstance.Robots.Add(robot);

        int i = 2;

        for (i = 2; i < 5; i++)
        {
            var cube = new Cube(room.SimInstance);
            room.SimInstance.NamedBodies.Add("cube" + i, cube.GetMainBodyReference());
        }
    }
}