using System.Numerics;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;

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
        var ground = new Ground(room, visualInfo: new VisualInfo() { Color = "#222" });

        // Walls
        float wallsize = 15;
        var wall1 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, -wallsize / 2), Quaternion.Identity, true, nameOverride: "wall1");
        var wall2 = new Cube(room, wallsize, 1, 1, new Vector3(0, 0.5f, wallsize / 2), Quaternion.Identity, true, nameOverride: "wall2");
        var wall3 = new Cube(room, 1, 1, wallsize + 1, new Vector3(-wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall3");
        var wall4 = new Cube(room, 1, 1, wallsize + 1, new Vector3(wallsize / 2, 0.5f, 0), Quaternion.Identity, true, nameOverride: "wall4");

        // Demo robots
        var robot = new ParallaxRobot(room);
        var robot2 = new ParallaxRobot(room);

        for (int i = 0; i < 3; i++)
        {
            var cube = new Cube(room, visualInfo: new VisualInfo() { Color = "#B85" });
        }
    }
}