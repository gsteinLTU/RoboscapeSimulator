using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

class DefaultEnvironment : EnvironmentConfiguration
{
    public static new string Name = "Default";
    public static new string ID = "default";
    public static new string Description = "The default environment";

    public new static void Setup(ref SimulationInstance sim)
    {

        // Ground
        var groundHandle = sim.Simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 0), new CollidableDescription(sim.Simulation.Shapes.Add(new Box(200, 1, 200)), 0.1f)));
        sim.NamedStatics.Add("ground", sim.Simulation.Statics.GetStaticReference(groundHandle));

        // Demo robot
        var robot = new ParallaxRobot(sim);
        sim.NamedBodies.Add("robot_" + Robot.BytesToHexstring(robot.MacAddress, ""), robot.MainBodyReference);
        // NamedBodies.Add("wheelL", Simulation.Bodies.GetBodyReference(robot.LWheel));
        // NamedBodies.Add("wheelR", Simulation.Bodies.GetBodyReference(robot.RWheel));
        // NamedBodies.Add("wheelRear", Simulation.Bodies.GetBodyReference(robot.RearWheel));

        sim.Robots.Add(robot);

        int i = 2;

        for (i = 2; i < 5; i++)
        {
            var cube = new Cube(sim);
            sim.NamedBodies.Add("cube" + i, cube.GetMainBodyReference());
        }
    }
}