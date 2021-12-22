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
        var ground = new Ground(room);
        room.SimInstance.NamedStatics.Add(ground.Name, ground.StaticReference);
        room.SimInstance.Entities.Add(ground);

        // Demo robot
        var robot = new ParallaxRobot(room);
        room.SimInstance.NamedBodies.Add("robot_" + Robot.BytesToHexstring(robot.MacAddress, ""), robot.MainBodyReference);
        // NamedBodies.Add("wheelL", Simulation.Bodies.GetBodyReference(robot.LWheel));
        // NamedBodies.Add("wheelR", Simulation.Bodies.GetBodyReference(robot.RWheel));
        // NamedBodies.Add("wheelRear", Simulation.Bodies.GetBodyReference(robot.RearWheel));
        room.SimInstance.Entities.Add(robot);

        for (int i = 0; i < 3; i++)
        {
            var cube = new Cube(room, visualInfo: "#A74");
            room.SimInstance.NamedBodies.Add(cube.Name, cube.GetMainBodyReference());
            room.SimInstance.Entities.Add(cube);
        }
    }
}