using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Timers;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

public class SimulationInstance : IDisposable
{
    Dictionary<string, StaticReference> NamedStatics = new();
    Dictionary<string, BodyReference> NamedBodies = new();

    List<Robot> Robots = new();

    public Simulation Simulation;

    /// <summary>
    /// Gets the buffer pool used by the demo's simulation.
    /// </summary>
    public BufferPool BufferPool { get; private set; }

    internal CollidableProperty<BodyCollisionProperties> Properties = new();

    public SimulationInstance()
    {
        Properties = new CollidableProperty<BodyCollisionProperties>();
        BufferPool = new BufferPool();
        Simulation = Simulation.Create(BufferPool, new SimulationInstanceCallbacks() { Properties = Properties }, new SimulationInstanceIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionFirstTimestepper());

        // Ground
        var groundHandle = Simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 0), new CollidableDescription(Simulation.Shapes.Add(new Box(200, 1, 200)), 0.1f)));
        NamedStatics.Add("ground", Simulation.Statics.GetStaticReference(groundHandle));

        // Demo robot
        var robot = new ParallaxRobot(this);
        NamedBodies.Add("robot", robot.MainBodyReference);
        // NamedBodies.Add("wheelL", Simulation.Bodies.GetBodyReference(robot.LWheel));
        // NamedBodies.Add("wheelR", Simulation.Bodies.GetBodyReference(robot.RWheel));
        // NamedBodies.Add("wheelRear", Simulation.Bodies.GetBodyReference(robot.RearWheel));

        Robots.Add(robot);

        int i = 2;

        for (i = 2; i < 5; i++)
        {
            var cube = new Cube(this);
            NamedBodies.Add("cube" + i, cube.GetMainBodyReference());
        }

        // var timer = new System.Timers.Timer(1000);

        // timer.Elapsed += (source, e) =>
        // {
        //     var cube = new Cube(Simulation);
        //     NamedBodies.Add("cube" + i++, cube.GetMainBodyReference());
        // };

        // timer.Start();
    }

    bool disposed;
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            Simulation.Dispose();
            BufferPool.Clear();
        }
    }

    public void Update(float dt)
    {
        Simulation.Timestep(dt);
        foreach (var robot in Robots)
        {
            robot.Update(dt);
        }
    }

    public Dictionary<String, BodyInfo> GetBodies()
    {
        var output = new Dictionary<String, BodyInfo>();

        foreach (var kvp in NamedStatics)
        {
            output.Add(kvp.Key, new BodyInfo
            {
                label = kvp.Key,
                pos = {
                    x = kvp.Value.Pose.Position.X,
                    y = kvp.Value.Pose.Position.Y,
                    z = kvp.Value.Pose.Position.Z
                },
                angle = kvp.Value.Pose.Orientation,
                width = kvp.Value.BoundingBox.Max.X - kvp.Value.BoundingBox.Min.X,
                height = kvp.Value.BoundingBox.Max.Y - kvp.Value.BoundingBox.Min.Y,
                depth = kvp.Value.BoundingBox.Max.Z - kvp.Value.BoundingBox.Min.Z,
            });
        }

        foreach (var kvp in NamedBodies)
        {
            output.Add(kvp.Key, new BodyInfo
            {
                label = kvp.Key,
                pos = {
                    x = kvp.Value.Pose.Position.X,
                    y = kvp.Value.Pose.Position.Y,
                    z = kvp.Value.Pose.Position.Z
                },
                angle = kvp.Value.Pose.Orientation,
                width = kvp.Value.BoundingBox.Max.X - kvp.Value.BoundingBox.Min.X,
                height = kvp.Value.BoundingBox.Max.Y - kvp.Value.BoundingBox.Min.Y,
                depth = kvp.Value.BoundingBox.Max.Z - kvp.Value.BoundingBox.Min.Z,
                image = kvp.Key.Contains("robot") ? "parallax_robot" : null
            });
        }

        return output;
    }

}

[Serializable]
public struct BodyInfo
{
    public string label;
    public Vec3 pos;
    public Vec3 vel;
    public Quaternion angle;
    public float anglevel;
    public float width;
    public float height;
    public float depth;
    public string image;
}

[Serializable]
public struct Vec3
{
    public float x;
    public float y;
    public float z;
}
