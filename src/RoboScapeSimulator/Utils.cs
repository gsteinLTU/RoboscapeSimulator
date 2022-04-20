using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities.Memory;
namespace RoboScapeSimulator
{
    public static class Utils
    {

        public static void ExtractYawPitchRoll(this Quaternion r, out float yaw, out float pitch, out float roll)
        {
            yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
            roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
        }

        /// <summary>
        /// Returns a random point on a circle on the XZ plane
        /// </summary>
        /// <param name="radius">Radius of circle</param>
        /// <param name="height">Additional Y-value of circle</param>
        /// <param name="center">Center of circle, (0,0,0) if null</param>
        /// <returns></returns>
        public static Vector3 PointOnCircle(this Random rng, float radius = 1, float height = 0, Vector3? center = null)
        {
            return new Vector3(radius * MathF.Cos(rng.NextSingle() * 2 * MathF.PI), height, radius * MathF.Sin(rng.NextSingle() * 2 * MathF.PI)) + (center ?? Vector3.Zero);
        }


        /// <summary>
        /// Returns a random point on a circle on the XZ plane
        /// </summary>
        /// <param name="radius">Radius of circle</param>
        /// <param name="height">Additional Y-value of circle</param>
        /// <param name="center">Center of circle, (0,0,0) if null</param>
        /// <returns></returns>
        public static Vector3 PointOnLine(this Random rng, Vector3 p1, Vector3 p2)
        {
            var d = p2 - p1;
            return p1 + d * rng.NextSingle();
        }

        /// <summary>
        /// Helper function to print a JToken
        /// </summary>
        public static void PrintJSON(JsonNode token)
        {
            if (token != null)
            {
                Debug.WriteLine(JsonSerializer.Serialize(token, new JsonSerializerOptions() { IncludeFields = true, Converters = { new SmallerFloatFormatConverter() } }));
            }
        }

        /// <summary>
        /// Helper function to print a JToken
        /// </summary>
        public static void PrintJSONArray(JsonNode[] tokens)
        {
            Array.ForEach(tokens, PrintJSON);
        }

        public static void SendAsJSON<T>(Node.Socket socket, string eventName, T data)
        {
            if (data != null)
            {
                try
                {
                    socket.Emit(eventName, JsonSerializer.Serialize(data, new JsonSerializerOptions() { IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new SmallerFloatFormatConverter() } }));
                }
                catch (Exception e)
                {
                    if (data is IDictionary<string, object> dict)
                    {
                        foreach (var entry in dict)
                        {
                            Console.WriteLine("\t" + entry.Key + ": " + entry.Value.ToString());
                        }
                    }
                    else
                    {
                        Trace.TraceError("Data: " + data.ToString());
                    }
                    Trace.TraceError(e.ToString());
                }
            }
            else
            {
                socket.Emit(eventName);
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

        private class SmallerFloatFormatConverter : JsonConverter<float>
        {
            public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetSingle();

            public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
            {
                if (MathF.Abs(value) < 0.001f)
                {
                    writer.WriteStringValue("0");
                }
                else
                {
                    writer.WriteStringValue(string.Format("{0:G" + (int)Math.Max(4, Math.Round(4 + MathF.Log10(MathF.Abs(value)))) + "}", value));
                }
            }
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
}