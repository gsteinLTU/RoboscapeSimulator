using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace RoboScapeSimulator.Entities.Robots
{
    /// <summary>
    /// Base class for any RoboScape robot, subclasses must implement actuators/sensors
    /// </summary>
    public abstract class Robot : DynamicEntity, IResettable
    {
        /// <summary>
        /// Reference to the simulation this robot is inside of
        /// </summary>
        internal Simulation simulation;

        /// <summary>
        /// Reference to the room this robot is inside of
        /// </summary>
        internal Room room;

        /// <summary>
        /// Position where robot was created
        /// </summary>
        internal Vector3 _initialPosition;

        /// <summary>
        /// Orientation where robot was created
        /// </summary>
        internal Quaternion _initialOrientation;

        /// <summary>
        /// Stopwatch for keeping track of this Robot's lifetime
        /// </summary>
        internal Stopwatch time = new();

        /// <summary>
        /// Instantiate a Robot inside a given simulation instance
        /// </summary>
        /// <param name="room">Room this Robot exists inside</param>
        public Robot(Room room, in Vector3? position = null, in Quaternion? rotation = null, in Vector3? size = null, float mass = 2, in VisualInfo? visualInfo = null, float spawnHeight = 0.4f)
        {
            this.room = room;
            simulation = room.SimInstance.Simulation;
            var rng = new Random();

            Box box;

            if (visualInfo != null)
            {
                VisualInfo = (VisualInfo)visualInfo;
            }
            else
            {
                VisualInfo = new VisualInfo()
                {
                    ModelName = "parallax_robot.gltf"
                };
            }

            if (size == null)
            {
                box = new Box(0.10f, 0.1f, 0.15f);
            }
            else
            {
                var tempSize = size.GetValueOrDefault();
                box = new Box(tempSize.Z, tempSize.Y, tempSize.X);
            }

            var boxInertia = box.ComputeInertia(mass);

            var bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(position ?? new Vector3(rng.Next(-5, 5), spawnHeight, rng.Next(-5, 5)), boxInertia, new CollidableDescription(simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0)));
            BodyReference = simulation.Bodies.GetBodyReference(bodyHandle);

            Width = BodyReference.BoundingBox.Max.X - BodyReference.BoundingBox.Min.X;
            Height = BodyReference.BoundingBox.Max.Y - BodyReference.BoundingBox.Min.Y;
            Depth = BodyReference.BoundingBox.Max.Z - BodyReference.BoundingBox.Min.Z;

            if (rotation == null)
            {
                BodyReference.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI);
            }
            else
            {
                BodyReference.Pose.Orientation = rotation.GetValueOrDefault();
            }

            _initialPosition = BodyReference.Pose.Position;
            _initialOrientation = BodyReference.Pose.Orientation;

            SetupRobot();
            time.Start();

            room.SimInstance.NamedBodies.Add("robot_" + BytesToHexstring(MacAddress, ""), BodyReference);
            Name = "robot_" + BytesToHexstring(MacAddress, "");
            room.SimInstance.Entities.Add(this);
        }

        /// <summary>
        /// Time between heartbeats sent to server
        /// </summary>
        public uint HeartbeatPeriod = 1;

        /// <summary>
        /// Socket used to talk to server for this Robot
        /// </summary>
        internal UdpClient? socket = null;

        /// <summary>
        /// Simulated MAC address of this Robot, used for identification with server
        /// </summary>
        public byte[] MacAddress = Array.Empty<byte>();

        private string? _id = null;

        public string ID
        {
            get
            {
                if (_id == null)
                {
                    _id = BytesToHexstring(MacAddress, "");
                }

                return _id;
            }
        }

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
        private readonly char[] nonDelayedMessages = { 'n', 'L', 'I', 'R', 'T' };

        /// <summary>
        /// Time previous heartbeat message was sent
        /// </summary>
        private float lastHeartbeat = -1;

        /// <summary>
        /// Methods to use as handlers for message types received by this robot
        /// </summary>
        public readonly Dictionary<char, Action<byte[]>> MessageHandlers = new();

        public event EventHandler? OnReset;

        /// <summary>
        /// Add a message handler for a certain message type
        /// </summary>
        /// <param name="messageCode">char code of message type</param>
        /// <param name="onMessage">Function to run when message is received, input to function is message as byte array</param>
        public void AddHandler(char messageCode, Action<byte[]> onMessage)
        {
            if (MessageHandlers.ContainsKey(messageCode))
            {
                throw new Exception($"Message code {messageCode} already has handler assigned");
            }

            MessageHandlers.Add(messageCode, onMessage);
        }

        /// <summary>
        /// Remove the handler for a message type
        /// </summary>
        /// <param name="messageCode">char code of message type</param>
        public void RemoveHandler(char messageCode)
        {
            MessageHandlers.Remove(messageCode);
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

            Trace.WriteLine($"Creating robot with MAC {BytesToHexstring(MacAddress, ":")} at initial position {_initialPosition}");
        }

        /// <summary>
        /// Generates a random array of bytes resembling a MAC address
        /// </summary>
        /// <returns>6 random bytes</returns>
        public static byte[] GenerateRandomMAC()
        {
            Random rng = new();

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

            Debug.WriteLine(BytesToHexstring(finalMessageBytes));

            socket?.SendAsync(finalMessageBytes, finalMessageBytes.Length);
        }

        public static string BytesToHexstring(byte[] bytes, string separator = " ")
        {
            return string.Join(separator, bytes.Select(b => b.ToString("x2")).ToArray());
        }

        // Update is called once per frame
        public override void Update(float dt)
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
                IPEndPoint? remoteEP = null;
                var msg = socket.Receive(ref remoteEP);

                Debug.WriteLine($"Message from {remoteEP.Address}: {BytesToHexstring(msg)}");

                room.LastInteractionTime = DateTime.Now;

                // Pass message to handler, if exists
                char messageCode = (char)msg[0];
                if (MessageHandlers.ContainsKey(messageCode))
                {
                    if (nonDelayedMessages.Contains(messageCode))
                    {
                        MessageHandlers[messageCode](msg);
                    }
                    else if (MinTimeBetweenMessages <= 0 || time.Elapsed.TotalSeconds - lastMessageTime > MinTimeBetweenMessages)
                    {
                        lastMessageTime = (float)time.Elapsed.TotalSeconds;
                        MessageHandlers[messageCode](msg);
                    }
                }
                else
                {
                    Trace.WriteLine($"No message handler for message with code {messageCode}");
                }

                // Return message to server
                SendRoboScapeMessage(msg);
            }
        }

        public new void Dispose()
        {
            //GameManager.Instance.LiveRobots.Remove(gameObject);

            // Clean up and remove robot object
            socket?.Close();

            base.Dispose();
        }

        public virtual void Reset()
        {
            // Put robot in initial position
            // Later scenarios may provide more complex handling for this
            BodyReference.Pose.Position = _initialPosition;
            BodyReference.Pose.Orientation = _initialOrientation;
            BodyReference.Velocity.Linear = new Vector3();
            BodyReference.Velocity.Angular = new Vector3();

            time.Restart();
            lastMessageTime = 0;
            SendRoboScapeMessage(new byte[] { (byte)'I' });

            OnReset?.Invoke(this, EventArgs.Empty);

            Debug.WriteLine($"Reset Robot");
        }
    }
}