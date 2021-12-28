using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

public class SimulationInstance : IDisposable
{
    /// <summary>
    /// References to static bodies in the scene
    /// </summary>
    internal Dictionary<string, StaticReference> NamedStatics = new();

    /// <summary>
    /// References to moving bodies in the scene
    /// </summary>
    internal Dictionary<string, BodyReference> NamedBodies = new();

    internal List<Entity> Entities = new();

    internal IEnumerable<Robot> Robots => Entities.Where(e => e is Robot).Cast<Robot>();

    public Simulation Simulation;

    /// <summary>
    /// Gets the buffer pool used by the demo's simulation.
    /// </summary>
    public BufferPool BufferPool { get; private set; }

    internal CollidableProperty<BodyCollisionProperties> Properties = new();

    /// <summary>
    /// Time elapsed in this simulation
    /// </summary>
    public float Time = 0;

    public SimulationInstance()
    {
        Properties = new CollidableProperty<BodyCollisionProperties>();
        BufferPool = new BufferPool();
        Simulation = Simulation.Create(BufferPool, new SimulationInstanceCallbacks() { Properties = Properties }, new SimulationInstanceIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionFirstTimestepper());
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

    /// <summary>
    /// Resets all resettable objects in the environment
    /// </summary>
    public void Reset()
    {
        ((List<IResettable>)Entities.Where(e => e is IResettable)).ForEach(e => e.Reset());
    }

    /// <summary>
    /// Update the simulation
    /// </summary>
    /// <param name="dt">Delta time in s</param>
    public void Update(float dt)
    {
        Time += dt;
        Simulation.Timestep(dt);
        foreach (var entity in Entities)
        {
            entity.Update(dt);
        }
    }

    public Dictionary<string, object> GetBodies(bool onlyAwake = false)
    {
        var output = new Dictionary<string, object>();

        if (!onlyAwake)
        {
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
                    width = kvp.Value.BoundingBox.Max.Z - kvp.Value.BoundingBox.Min.Z,
                    height = kvp.Value.BoundingBox.Max.Y - kvp.Value.BoundingBox.Min.Y,
                    depth = kvp.Value.BoundingBox.Max.X - kvp.Value.BoundingBox.Min.X,
                    image = kvp.Key.Contains(':') ? kvp.Key.Split(':').Last() : null
                });
            }
        }

        foreach (var kvp in NamedBodies)
        {
            if (!onlyAwake || kvp.Value.Awake)
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
                    width = kvp.Value.BoundingBox.Max.Z - kvp.Value.BoundingBox.Min.Z,
                    height = kvp.Value.BoundingBox.Max.Y - kvp.Value.BoundingBox.Min.Y,
                    depth = kvp.Value.BoundingBox.Max.X - kvp.Value.BoundingBox.Min.X,
                    image = kvp.Key.StartsWith("robot") ? "parallax_robot" : (kvp.Key.Contains(':') ? kvp.Key.Split(':').Last() : null),
                    vel = kvp.Value.Velocity.Linear
                });
            }
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
    public string? image;
}

[Serializable]
public struct Vec3
{
    public static implicit operator Vec3(Vector3 vector3) => new Vec3() { x = vector3.X, y = vector3.Y, z = vector3.Z };
    public float x;
    public float y;
    public float z;
}
