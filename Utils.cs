using System.Numerics;
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
}