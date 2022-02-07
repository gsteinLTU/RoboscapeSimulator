using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Server.Client;

public static class Utils
{
    static JsonSerializer serializer = new();

    public static void ExtractYawPitchRoll(this Quaternion r, out float yaw, out float pitch, out float roll)
    {
        yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
        pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
        roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
    }

    /// <summary>
    /// Helper function to print a JToken
    /// </summary>
    public static void printJSON(JToken token)
    {
        using (var writer = new StringWriter())
        {
            serializer.Serialize(writer, token);
            Console.WriteLine(writer.ToString());
        }
    }

    /// <summary>
    /// Helper function to print a JToken
    /// </summary>
    public static void printJSONArray(JToken[] tokens)
    {
        Array.ForEach(tokens, printJSON);
    }

    public static void sendAsJSON<T>(SocketIOSocket socket, string eventName, T data)
    {
        using (var writer = new JTokenWriter())
        {
            serializer.Serialize(writer, data);
            socket.Emit(eventName, writer.Token);
        }
    }

    public unsafe static bool QuickRayCast(Simulation simulation, Vector3 origin, Vector3 direction, float maxRange = 300)
    {
        int intersectionCount = 0;
        simulation.BufferPool.Take(1, out Buffer<RayHit> results);

        HitHandler hitHandler = new()
        {
            Hits = results,
            IntersectionCount = &intersectionCount
        };

        simulation.RayCast(origin, direction, maxRange / 100f, ref hitHandler);
        simulation.BufferPool.Return(ref results);
        return intersectionCount > 0;
    }
}

public struct RayHit
{
    public Vector3 Normal;
    public float T;
    public CollidableReference Collidable;
    public bool Hit;
}

public unsafe struct HitHandler : IRayHitHandler
{
    public Buffer<RayHit> Hits;
    public int* IntersectionCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable, int childIndex)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
    {
        maximumT = t;
        ref var hit = ref Hits[ray.Id];
        // if (t < hit.T)
        // {
        //     if (hit.T != float.MaxValue)
        ++*IntersectionCount;
        hit.Normal = normal;
        hit.T = t;
        hit.Collidable = collidable;
        hit.Hit = true;
        // }
    }
}