using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

class DemoEnvironment : EnvironmentConfiguration
{
    public DemoEnvironment()
    {
        Name = "Demo 2021";
        ID = "demo";
        Description = "The demo environment";
    }

    public override void Setup(Room room)
    {
        Console.WriteLine("Setting up demo 2021 environment");

        // Ground
        var ground = new Ground(room);
        room.SimInstance.NamedStatics.Add("ground", ground.StaticReference);

        // Walls
        float wallsize = 15;

        var wall1 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true);
        room.SimInstance.NamedBodies.Add("wall1", wall1.GetMainBodyReference());

        var wall2 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, wallsize / 2), Quaternion.Identity, true);
        room.SimInstance.NamedBodies.Add("wall2", wall2.GetMainBodyReference());

        var wall3 = new Cube(room, 1, 1, wallsize + 1, new Vector3(-wallsize / 2, 0.5f, 0), Quaternion.Identity, true);
        room.SimInstance.NamedBodies.Add("wall3", wall3.GetMainBodyReference());

        var wall4 = new Cube(room, 1, 1, wallsize + 1, new Vector3(wallsize / 2, 0.5f, 0), Quaternion.Identity, true);
        room.SimInstance.NamedBodies.Add("wall4", wall4.GetMainBodyReference());

        // Demo robots
        var robot = new ParallaxRobot(room);
        room.SimInstance.NamedBodies.Add("robot_" + Robot.BytesToHexstring(robot.MacAddress, ""), robot.MainBodyReference);

        var robot2 = new ParallaxRobot(room);
        room.SimInstance.NamedBodies.Add("robot_" + Robot.BytesToHexstring(robot2.MacAddress, ""), robot2.MainBodyReference);

        room.SimInstance.Robots.Add(robot);
        room.SimInstance.Robots.Add(robot2);

        for (int i = 2; i < 5; i++)
        {
            var cube = new Cube(room);
            room.SimInstance.NamedBodies.Add("cube" + i, cube.GetMainBodyReference());
        }
    }
}