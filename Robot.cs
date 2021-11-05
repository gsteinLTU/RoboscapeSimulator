using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

/// <summary>
/// Base class for any RoboScape robot, subclasses must implement actuators/sensors
/// </summary>
abstract class Robot : IDisposable
{
    /// <summary>
    /// Reference to the simulation this robot is inside of
    /// </summary>
    internal Simulation simulation;

    /// <summary>
    /// Position where robot was created
    /// </summary>
    internal Vector3 _initialPosition;

    /// <summary>
    /// Orientation where robot was created
    /// </summary>
    internal Quaternion _initialOrientation;

    /// <summary>
    /// Reference to main body of robot in physics engine
    /// </summary>
    internal BodyReference bodyReference;

    /// <summary>
    /// Reference to main body of robot in physics engine
    /// </summary>
    public BodyReference MainBodyReference
    {
        get
        {
            return bodyReference;
        }
    }

    /// <summary>
    /// Stopwatch for keeping track of this Robot's lifetime
    /// </summary>
    internal Stopwatch time = new Stopwatch();

    /// <summary>
    /// Instantiate a Robot inside a given simulation instance
    /// </summary>
    /// <param name="simulationInstance">SimulationInstance this Robot exists inside</param>
    public Robot(SimulationInstance simulationInstance)
    {
        simulation = simulationInstance.Simulation;
        var rng = new Random();
        var box = new Box(0.35f, 0.28f, 0.15f);
        box.ComputeInertia(3, out var boxInertia);
        var bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(new Vector3(rng.Next(-5, 5), 0.4f, rng.Next(-5, 5)), boxInertia, new CollidableDescription(simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0)));
        bodyReference = simulation.Bodies.GetBodyReference(bodyHandle);
        // bodyReference.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI);
        _initialPosition = bodyReference.Pose.Position;
        _initialOrientation = bodyReference.Pose.Orientation;

        SetupRobot();
        time.Start();
    }

    /// <summary>
    /// Time between heartbeats sent to server
    /// </summary>
    public uint HeartbeatPeriod = 1;

    /// <summary>
    /// Socket used to talk to server for this Robot
    /// </summary>
    internal UdpClient socket = null;

    /// <summary>
    /// Simulated MAC address of this Robot, used for identification with server
    /// </summary>
    public byte[] MacAddress;

    /// <summary>
    /// Threshold to reject messages sent too quickly, allows for DoS cybersecurity example, set to 0 to disable
    /// </summary>
    public float MinTimeBetweenMessages = 1f / 45f;

    /// <summary>
    /// Time last message was received
    /// </summary>
    private float lastMessageTime = 0;

    /// <summary>
    /// Message types to not enforce minimum times for
    /// </summary>
    private char[] nonDelayedMessages = { 'n', 'L', 'I' };

    /// <summary>
    /// Time previous heartbeat message was sent
    /// </summary>
    private float lastHeartbeat = -1;

    /// <summary>
    /// Methods to use as handlers for message types received by this robot
    /// </summary>
    private Dictionary<char, Action<byte[]>> messageHandlers = new();

    /// <summary>
    /// Add a message handler for a certain message type
    /// </summary>
    /// <param name="messageCode">char code of message type</param>
    /// <param name="onMessage">Function to run when message is received, input to function is message as byte array</param>
    public void AddHandler(char messageCode, Action<byte[]> onMessage)
    {
        if (messageHandlers.ContainsKey(messageCode))
        {
            throw new Exception($"Message code {messageCode} already has handler assigned");
        }

        messageHandlers.Add(messageCode, onMessage);
    }

    /// <summary>
    /// Remove the handler for a message type
    /// </summary>
    /// <param name="messageCode">char code of message type</param>
    public void RemoveHandler(char messageCode)
    {
        messageHandlers.Remove(messageCode);
    }

    private void SetupRobot()
    {
        socket = new UdpClient();

        // Remove port from host to make localhost use easier
        string host = SettingsManager.RoboScapeHostWithoutPort;

        socket.Connect(host, SettingsManager.RoboScapePort);

        if (MacAddress?.Length != 6)
        {
            MacAddress = GenerateRandomMAC();
        }

        Console.WriteLine($"Creating robot with MAC {BytesToHexstring(MacAddress, ":")} at initial position {_initialPosition}");
    }

    /// <summary>
    /// Generates a random array of bytes resembling a MAC address
    /// </summary>
    /// <returns>6 random bytes</returns>
    public static byte[] GenerateRandomMAC()
    {
        Random rng = new Random();

        byte[] result = new byte[6];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = (byte)rng.Next(0, 255);
        }

        // Not needed for RoboScape, but set MAC to be locally administered and unicast
        result[0] &= 0b11111110;
        result[0] |= 0b00000010;

        // Check last four digits for e and numbers
        var lastFour = BytesToHexstring(result.Reverse().Take(2).Reverse().ToArray(), "");
        var lastFourDigitCount = lastFour.Count(char.IsDigit);

        // Prevent NetsBlox leading zero truncation
        if (lastFour.StartsWith("0") && lastFourDigitCount == 4)
        {
            result[4] |= 0b00010001;
        }

        // Accidental float prevention
        if (lastFour.Contains('e') && lastFourDigitCount == 3)
        {
            result[4] |= 0b10001000;
        }

        return result;
    }

    /// <summary>
    /// Send a message to the RoboScape server from this Robot
    /// </summary>
    /// <param name="messageBytes">Message to send</param>
    public void SendRoboScapeMessage(byte[] messageBytes)
    {
        // Allocate room for MAC address and timestamp
        byte[] finalMessageBytes = new byte[messageBytes.Length + 10];

        // Add MAC to message
        MacAddress.CopyTo(finalMessageBytes, 0);

        // Add timestamp
        BitConverter.GetBytes((int)(1000 * (float)time.Elapsed.TotalSeconds)).CopyTo(finalMessageBytes, 6);

        // Add actual message
        if (messageBytes.Length > 0)
        {
            messageBytes.CopyTo(finalMessageBytes, 10);
        }

        Console.WriteLine(BytesToHexstring(finalMessageBytes));

        socket.SendAsync(finalMessageBytes, finalMessageBytes.Length);
    }

    public static string BytesToHexstring(byte[] bytes, string separator = " ")
    {
        return string.Join(separator, bytes.Select(b => b.ToString("x2")).ToArray());
    }

    // Update is called once per frame
    public virtual void Update(float dt)
    {
        // Setup robots
        if (socket == null)
        {
            SetupRobot();
        }

        if ((lastHeartbeat + HeartbeatPeriod) < time.Elapsed.TotalSeconds || lastHeartbeat < 0)
        {
            // Send heartbeat if due
            SendRoboScapeMessage(new[] { (byte)'I' });
            lastHeartbeat = (float)time.Elapsed.TotalSeconds;
        }

        if (socket?.Available > 0)
        {
            IPEndPoint remoteEP = null;
            var msg = socket.Receive(ref remoteEP);

            Console.WriteLine($"Message from {remoteEP.Address}: {BytesToHexstring(msg)}");

            // Pass message to handler, if exists
            char messageCode = (char)msg[0];
            if (messageHandlers.ContainsKey(messageCode))
            {
                if (nonDelayedMessages.Contains(messageCode))
                {
                    messageHandlers[messageCode](msg);
                }
                else if (MinTimeBetweenMessages <= 0 || time.Elapsed.TotalSeconds - lastMessageTime > MinTimeBetweenMessages)
                {
                    lastMessageTime = (float)time.Elapsed.TotalSeconds;
                    messageHandlers[messageCode](msg);
                }
            }
            else
            {
                Console.WriteLine($"No message handler for message with code {messageCode}");
            }

            // Return message to server
            SendRoboScapeMessage(msg);
        }
    }

    public void Dispose()
    {
        //GameManager.Instance.LiveRobots.Remove(gameObject);

        // Clean up and remove robot object
        socket.Close();
    }

    public void Reset()
    {
        // Put robot in initial position
        // Later scenarios may provide more complex handling for this
        bodyReference.Pose.Position = _initialPosition;
        bodyReference.Pose.Orientation = _initialOrientation;

        time.Restart();
        SendRoboScapeMessage(new byte[] { (byte)'I' });

        Console.WriteLine($"Reset Robot");
    }
}