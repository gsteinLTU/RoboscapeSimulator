using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EmbedIO;

namespace RoboScapeSimulator
{
    /// <summary>
    /// Various utility and helper functions
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Get Euler angles from a Quaternion
        /// </summary>
        public static void ExtractYawPitchRoll(this Quaternion r, out float yaw, out float pitch, out float roll)
        {
            yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
            roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
        }

        /// <summary>
        /// Clamp a value between a min and max
        /// </summary>
        /// <param name="val">Value to clamp</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <typeparam name="T">Type of values to compare</typeparam>
        /// <returns>val if between min and max, otherwise the closest bound</returns>
        internal static T Clamp<T>(T val, T min, T max) where T : IComparable
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }

            if (val.CompareTo(max) > 0)
            {
                return max;
            }

            return val;
        }

        /// <summary>
        /// Serialize an object to JSON and then send it as a response
        /// </summary>
        /// <typeparam name="T">Type of data to send</typeparam>
        /// <param name="context"></param>
        /// <param name="data">Data to serialize and send</param>
        /// <param name="options">Optional serializer options</param>
        /// <returns></returns>
        public static Task SendAsJSON<T>(this IHttpContext context, T data, JsonSerializerOptions? options = null)
        {
            string output = JsonSerializer.Serialize(data, options ?? new JsonSerializerOptions() { IncludeFields = true });
            return context.SendStringAsync(output, "application/json", Encoding.Default);
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
        /// Get evenly spaced points on a circle
        /// </summary>
        /// <param name="count">Number of points</param>
        /// <param name="radius">Radius of circle</param>
        /// <param name="height">Additional Y-value of circle</param>
        /// <param name="center">Center of circle, (0,0,0) if null</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> PointsOnCircle(int count, float radius = 1, float height = 0, Vector3? center = null)
        {
            return Enumerable.Range(1, count).Select(i => (MathF.PI * 2f / count) * i).Select(i => new Vector3(radius * MathF.Cos(i), height, radius * MathF.Sin(i)) + (center ?? Vector3.Zero));
        }

        /// <summary>
        /// Returns a random point outside a radius, but inside another radius, on the XZ plane
        /// </summary>
        /// <param name="innerRadius">Inner radius of area</param>
        /// <param name="outerRadius">Outer radius of area</param>
        /// <param name="height">Additional Y-value of area</param>
        /// <param name="center">Center of area, (0,0,0) if null</param>
        /// <returns></returns>
        public static Vector3 PointOnAnnulus(this Random rng, float innerRadius = 1, float outerRadius = 2, float height = 0, Vector3? center = null)
        {
            float theta = rng.NextSingle() * 2f * MathF.PI;
            float dist = MathF.Sqrt(rng.NextSingle() * (outerRadius * outerRadius - innerRadius * innerRadius) + innerRadius * innerRadius);
            return new Vector3(dist * MathF.Cos(theta), height, dist * MathF.Sin(theta)) + (center ?? Vector3.Zero);
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

        /// <summary>
        /// Serialize data and then send it as an event
        /// </summary>
        /// <typeparam name="T">Type of data to send</typeparam>
        /// <param name="socket">Socket to send to</param>
        /// <param name="eventName">Name of event to send</param>
        /// <param name="data">Data to serialize and send</param>
        public static void SendAsJSON<T>(Node.SocketBase socket, string eventName, T data)
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
                            Trace.WriteLine("\t" + entry.Key + ": " + entry.Value.ToString());
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

        /// <summary>
        /// Serialize data and then send it as an event to many sockets
        /// </summary>
        /// <typeparam name="T">Type of data to send</typeparam>
        /// <param name="sockets">Sockets to send to</param>
        /// <param name="eventName">Name of event to send</param>
        /// <param name="data">Data to serialize and send</param>
        public static void SendAsJSON<T>(IEnumerable<Node.Socket> sockets, string eventName, T data)
        {
            if (data != null)
            {
                try
                {
                    string serialized = JsonSerializer.Serialize(data, new JsonSerializerOptions() { IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new SmallerFloatFormatConverter() } });
                    foreach (var socket in sockets)
                    {
                        socket.Emit(eventName, serialized);
                    }
                }
                catch (Exception e)
                {
                    if (data is IDictionary<string, object> dict)
                    {
                        foreach (var entry in dict)
                        {
                            Trace.WriteLine("\t" + entry.Key + ": " + entry.Value.ToString());
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
                foreach (var socket in sockets)
                {
                    socket.Emit(eventName);
                }
            }
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

        /// <summary>
        /// Get the largest component of this vector
        /// </summary>
        public static float Max(this Vector3 vec){
            return MathF.Max(vec.X, MathF.Max(vec.Y, vec.Z));
        }

        /// <summary>
        /// Get the component-wise maximum of this Vector3 and another Vector3
        /// </summary>
        public static Vector3 Max(this Vector3 vec, Vector3 v2){
            return new Vector3(MathF.Max(vec.X, v2.X), MathF.Max(vec.Y, v2.Y), MathF.Max(vec.Z, v2.Z));
        }

        /// <summary>
        /// Get the smallest component of this vector
        /// </summary>
        public static float Min(this Vector3 vec){
            return MathF.Min(vec.X, MathF.Min(vec.Y, vec.Z));
        }

        /// <summary>
        /// Get the component-wise minimum of this Vector3 and another Vector3
        /// </summary>
        public static Vector3 Min(this Vector3 vec, Vector3 v2){
            return new Vector3(MathF.Min(vec.X, v2.X), MathF.Min(vec.Y, v2.Y), MathF.Min(vec.Z, v2.Z));
        }

        /// <summary>
        /// Clamp the components of this vector component-wise between two other vectors
        /// </summary>
        public static Vector3 Clamp(this Vector3 vec, Vector3 min, Vector3 max){
            vec.X = Clamp(vec.X, min.X, max.X);
            vec.Y = Clamp(vec.Y, min.Y, max.Y);
            vec.Z = Clamp(vec.Z, min.Z, max.Z);
            return vec;
        }

        /// <summary>
        /// Test if this vector is inside a cubic region defined by two corners
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="min">Lowest values of components</param>
        /// <param name="max">Largest values of components</param>
        /// <returns>If this Vector3's components are all between min and max.</returns>
        public static bool Inside(this Vector3 vec, Vector3 min, Vector3 max){
            return vec.X >= min.X && vec.Y >= min.Y && vec.Z >= min.Z &&
                vec.X <= max.X && vec.Y <= max.Y && vec.Z <= max.Z;
        }
    }
}