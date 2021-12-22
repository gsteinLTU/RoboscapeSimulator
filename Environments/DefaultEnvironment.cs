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

        const bool debugRobot = false;
        if (debugRobot)
        {
            room.SimInstance.NamedBodies.Add("wheelL", room.SimInstance.Simulation.Bodies.GetBodyReference(robot.LWheel));
            room.SimInstance.NamedBodies.Add("wheelR", room.SimInstance.Simulation.Bodies.GetBodyReference(robot.RWheel));
            room.SimInstance.NamedBodies.Add("wheelRear", room.SimInstance.Simulation.Bodies.GetBodyReference(robot.RearWheel));
        }

        room.SimInstance.Entities.Add(robot);

        for (int i = 0; i < 3; i++)
        {
            var cube = new Cube(room, visualInfo: "#B85");
            room.SimInstance.NamedBodies.Add(cube.Name, cube.GetMainBodyReference());
            room.SimInstance.Entities.Add(cube);
        }
    }
}