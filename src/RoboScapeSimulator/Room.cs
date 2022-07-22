using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using RoboScapeSimulator.Entities;
using RoboScapeSimulator.Entities.Robots;
using RoboScapeSimulator.Environments;
using RoboScapeSimulator.Node;

namespace RoboScapeSimulator
{
    /// <summary>
    /// A room shared between a set of users
    /// </summary>
    public class Room : IDisposable
    {
        /// <summary>
        /// List of sockets for users connected to this room
        /// </summary>
        public List<Node.Socket> activeSockets = new();

        /// <summary>
        /// Visible string used to identify this room
        /// </summary>
        public string Name;

        /// <summary>
        /// Extra string required to join the room, not intended for cryptographic security.
        /// </summary>
        public string Password;

        /// <summary>
        /// The simulated environment this Room represents
        /// </summary>
        public SimulationInstance SimInstance;

        /// <summary>
        /// Username/ID of this Room's creator
        /// </summary>
        public string? Creator;

        /// <summary>
        /// List of users who have been in this room
        /// </summary>
        public HashSet<string> Visitors = new();

        /// <summary>
        /// Time (in seconds) without interaction this room will stay alive for, default 15 minutes
        /// </summary>
        public float Timeout = 60 * 15;

        /// <summary>
        /// Previous time this Room was interacted with
        /// </summary>
        private long lastInteractionTime;

        /// <summary>
        /// When going inactive, how long should this room be kept in "suspended animation" until deletion
        /// </summary>
        public float MaxHibernateTime = 60 * 60 * 24;

        /// <summary>
        /// Event called when this Room enters the hibernating state
        /// </summary>
        public event EventHandler? OnHibernateStart;

        /// <summary>
        /// Event called when this Room resumes if currently hibernating
        /// </summary>
        public event EventHandler? OnHibernateEnd;

        /// <summary>
        /// Event called when this room is destroyed
        /// </summary>
        public event EventHandler? OnRoomClose;

        /// <summary>
        /// Event called when this room updates
        /// </summary>
        public event EventHandler<float>? OnUpdate;

        /// <summary>
        /// Event called when this room has a reset requested
        /// </summary>
        public event EventHandler? OnReset;

        /// <summary>
        /// ID of the environment used to launch this Room
        /// </summary>
        public string EnvironmentID;

        /// <summary>
        /// Time elapsed in the simulation instance
        /// </summary>
        public float Time { get => SimInstance.Time; }

        private float timeMultiplier = 1.0f;

        /// <summary>
        /// Allows a room to run at higher/lower speed, minimum 0.1, maximum 10, default 1
        /// </summary>
        public float TimeMultiplier { get => timeMultiplier; set => timeMultiplier = Math.Clamp(value, 0.1f, 10f); }

        private bool hibernating = false;

        /// <summary>
        /// Is the Room currently suspended
        /// </summary>
        public bool Hibernating
        {
            get => hibernating; set
            {
                if (hibernating != value)
                {
                    // Fire appropriate event
                    if (hibernating)
                    {
                        OnHibernateEnd?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        OnHibernateStart?.Invoke(this, EventArgs.Empty);
                    }

                    hibernating = value;
                }
            }
        }

        /// <summary>
        /// The EnvironmentConfiguration used to setup this Room
        /// </summary>
        readonly EnvironmentConfiguration? environmentConfiguration;

        /// <summary>
        /// Instantiate a Room
        /// </summary>
        /// <param name="name">Name of this Room, leave empty to be assigned a random name</param>
        /// <param name="password">Password to restrict entry to this Room with</param>
        /// <param name="environment">ID of EnvironmentConfiguration to setup this Room with</param>
        public Room(string name = "", string password = "", string environment = "default")
        {
            Trace.WriteLine($"Setting up room {name} with environment {environment}");
            SimInstance = new SimulationInstance();

            // Find requested environment (or use default)
            if (!Environments.Any((env) => env.ID == environment))
            {
                Trace.WriteLine($"Environment {environment} not found");
            }
            var env = Environments.Find((env) => env.ID == environment) ?? Environments[0];

            EnvironmentID = env.ID;

            // Create instance of requested environment
            environmentConfiguration = (EnvironmentConfiguration?)env.Clone();

            if (environmentConfiguration == null)
            {
                environmentConfiguration = new DefaultEnvironment();
            }

            environmentConfiguration.Setup(this);

            // Give randomized default name
            if (string.IsNullOrWhiteSpace(name))
            {
                Random random = new();
                Name = "Room" + random.Next(0, 1000000).ToString("X4");
            }
            else
            {
                Name = name;
            }

            Password = password;

            LastInteractionTime = Environment.TickCount64;

            OnHibernateStart += (o, e) =>
            {
                Trace.WriteLine($"Room {Name} is now hibernating");
            };

            OnHibernateEnd += (o, e) =>
            {
                Trace.WriteLine($"Room {Name} is no longer hibernating");
            };

            HandleResetAll();
            Trace.WriteLine("Room " + Name + " created.");
        }

        public void Dispose()
        {
            OnRoomClose?.Invoke(this, EventArgs.Empty);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Collect information about the room's metadata in a format sendable to the client as JSON
        /// </summary>
        /// <returns>Structure containing room metadata</returns>
        public Dictionary<string, object> GetInfo()
        {
            return new Dictionary<string, object>()
            {
                {"background", ""},
                {"time", Time}
            };
        }

        /// <summary>
        /// Send an event to all clients in the room
        /// </summary>
        /// <param name="eventName">Name of event</param>
        /// <param name="args">Arguments for event</param>
        public void SendToClients(string eventName, params object[] args)
        {
            lock (activeSockets)
            {
                foreach (var socket in activeSockets)
                {
                    Utils.SendAsJSON(socket, eventName, args);
                }
            }
        }

        /// <summary>
        /// Send an event to one client in the room
        /// </summary>
        /// <param name="socketID">ID of target socket</param>
        /// <param name="eventName">Name of event</param>
        /// <param name="args">Arguments for event</param>
        public void SendToClient(string socketID, string eventName, params object[] args)
        {
            lock (activeSockets)
            {
                foreach (var socket in activeSockets.Where(s => s.ID == socketID))
                {
                    Utils.SendAsJSON(socket, eventName, args);
                }
            }
        }

        /// <summary>
        /// Adds a socket to active list and sets up message listeners
        /// </summary>
        /// <param name="socket">Socket to add to active sockets list</param>
        /// <param name="username">Username of user joining</param>
        internal void AddSocket(Node.Socket socket, string? username)
        {
            LastInteractionTime = Environment.TickCount64;
            lock (activeSockets)
            {
                activeSockets.Add(socket);
            }
            socket.On("resetRobot", HandleResetRobot);
            socket.On("resetAll", HandleResetAll);
            socket.On("robotButton", HandleRobotButton);
            socket.On("claimRobot", HandleClaimRobot);

            if (username != null)
            {
                Visitors.Add(username);

                SendAPIUpdate();
            }
        }

        /// <summary>
        /// Handles a request to reset a robot
        /// </summary>
        /// <param name="args">Input from event</param>
        private void HandleResetRobot(Socket s, JsonNode[] args)
        {
            if (args.Length < 2)
            {
                return;
            }

            LastInteractionTime = Environment.TickCount64;
            string robotID = args[0].ToString();
            string userID = args[1].ToString();

            ResetRobot(robotID, userID);
        }

        /// <summary>
        /// Handles a request to reset the entire environment
        /// </summary>
        /// <param name="args">Input from event</param>
        private void HandleResetAll(Socket? s = null, JsonNode[]? args = null)
        {
            LastInteractionTime = Environment.TickCount64;

            foreach (var entity in SimInstance.Entities)
            {
                if (entity is IResettable resettable)
                {
                    resettable.Reset();
                }
            }

            OnReset?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles a request to press the button on a robot
        /// </summary>
        /// <param name="args">Input from event</param>
        private void HandleRobotButton(Socket s, JsonNode[] args)
        {
            if (args.Length < 3)
            {
                return;
            }

            string robotID = args[0].ToString();
            string userID = args[2].ToString();

            Robot? robot = SimInstance.Robots.FirstOrDefault(r => r?.ID == robotID, null);
            if (robot is ParallaxRobot parallaxRobot)
            {
                if (!robot.claimable || robot.claimedByUser == null || robot.claimedByUser == userID)
                {
                    parallaxRobot.OnButtonPress((bool)args[1]);
                }
            }
        }

        /// <summary>
        /// Handles a request to claim a robot
        /// </summary>
        /// <param name="args">Input from event</param>
        private void HandleClaimRobot(Socket s, JsonNode[] args)
        {
            if (args.Length < 3)
            {
                return;
            }

            string robotID = args[0].ToString();
            bool request = (bool)args[1];
            string userID = args[2].ToString();

            Robot? robot = SimInstance.Robots.FirstOrDefault(r => r?.ID == robotID, null);
            if (robot != null && robot.claimable)
            {
                if (request)
                {
                    if (robot.claimedByUser == null)
                    {
                        robot.claimedByUser = userID;
                        robot.claimedBySocket = s.ID;

                        // Send status to users
                        SendToClients("robotClaimed", robotID, userID, true);
                    }
                }
                else
                {
                    if (robot.claimedByUser == userID)
                    {
                        robot.claimedByUser = null;
                        robot.claimedBySocket = null;

                        // Send status to users
                        SendToClients("robotClaimed", robotID, userID, false);
                    }
                }

            }
        }

        /// <summary>
        /// Reset a robot based on its ID
        /// </summary>
        /// <param name="robotID">ID of robot to reset</param>
        /// <param name="userID">Optional, user ID requesting reset</param>
        public void ResetRobot(string robotID, string? userID = null)
        {
            Robot? robot = SimInstance.Robots.FirstOrDefault(r => r?.ID == robotID, null);
            if (robot != null)
            {
                if (!robot.claimable || robot.claimedByUser == null || robot.claimedByUser == userID)
                {
                    robot.Reset();
                }
            }
            else
            {
                Trace.WriteLine("Attempt to reset unknown robot " + robot);
            }
        }

        /// <summary>
        /// Removes a socket from active list and removes message listeners
        /// </summary>
        /// <param name="socket">Socket to remove from active sockets list</param>
        internal void RemoveSocket(Socket socket)
        {
            socket.Off("resetRobot", HandleResetRobot);
            socket.Off("resetAll", HandleResetAll);
            socket.Off("robotButton", HandleRobotButton);
            socket.Off("claimRobot", HandleClaimRobot);
            socket.Emit("roomLeft");

            // Kick out of any claimed robots
            foreach (var robot in SimInstance.Robots)
            {
                if (robot.claimedBySocket == socket.ID)
                {
                    SendToClients("robotClaimed", robot.ID, robot.claimedByUser ?? "", false);
                    robot.claimedByUser = null;
                    robot.claimedBySocket = null;
                }
            }

            lock (activeSockets)
            {
                activeSockets.Remove(socket);

                // Stop running room when no users
                if (activeSockets.Count == 0)
                {
                    Hibernating = true;
                }
            }
        }

        /// <summary>
        /// Lists the available environment types in a format sendable to the client as JSON
        /// </summary>
        /// <returns>Structure representing information about avaialble environment types</returns>
        public static List<Dictionary<string, object>> ListEnvironments()
        {
            return Environments.Select(
                (environmentType) => new Dictionary<string, object> {
                { "Name", environmentType.Name },
                { "ID", environmentType.ID },
                {"Description", environmentType.Description},
                {"Category", environmentType.Category}
            }).ToList();
        }

        /// <summary>
        /// Available environment types
        /// </summary>
        internal static List<EnvironmentConfiguration> Environments = new()
        {
            new DefaultEnvironment(),
            new WallEnvironment(),
            new SquareDrivingEnvironment(),
            // Multi-color robot environments
            new MultiColorRobotsEnvironment(1),
            new MultiColorRobotsEnvironment(2),
            new MultiColorRobotsEnvironment(3),
            new MultiColorRobotsEnvironment(),
            new MultiColorRobotsEnvironment(1, true),
            new MultiColorRobotsEnvironment(2, true),
            new MultiColorRobotsEnvironment(3, true),
            new MultiColorRobotsEnvironment(4, true),
            new MultiColorRobotsEnvironment(1, true, true, true),
            new MultiColorRobotsEnvironment(2, true, true, true),
            new MultiColorRobotsEnvironment(3, true, true, true),
            new MultiColorRobotsEnvironment(4, true, true, true),
            new ObstacleCourseEnvironment(),
            new TableEnvironment(2, 1, true),
            new TableEnvironment(2, 2, true),
            new TreasureHuntEnvironment(),
            // LIDAR roads
            new LIDARRoadEnvironment(),
            new LIDARRoadEnvironment(LIDARRoadEnvironment.Courses.Hard),
            new LIDARRoadEnvironment(LIDARRoadEnvironment.Courses.VeryHard),
            new WaypointNavigationEnvironment(),
            new WaypointNavigationEnvironment(3),
            new BoxPushingRace(),
            new BoxPushingRace(true),
            new RobotInterceptEnvironment(RobotInterceptEnvironment.Difficulties.Easy),
            new RobotInterceptEnvironment(RobotInterceptEnvironment.Difficulties.Hard),
            new FinalChallengeEnvironment(),
            new FinalChallengeEnvironment(true),
            new PhysicsTestEnvironment(),
        };

        public DateTime LastInteractionDateTime
        {
            get => Program.StartDateTime.AddMilliseconds(lastInteractionTime);
            set
            {
                lastInteractionTime = (long)(value - Program.StartDateTime).TotalMilliseconds;
            }
        }

        public long LastInteractionTime
        {
            get => lastInteractionTime;
            set
            {
                lastInteractionTime = value;

                // Wake up if sleeping
                Hibernating = false;
            }
        }

        /// <summary>
        /// Client-relevant information about a Room
        /// </summary>
        [Serializable]
        public struct RoomInfo
        {
            public string name;

            public bool hasPassword;

            public string environment;

            public DateTime lastInteractionTime;

            public bool isHibernating;

            public string creator;

            public List<string> visitors;
        }

        /// <summary>
        /// Get the client-relevant information about this room
        /// </summary>
        /// <returns>Struct of information about this room</returns>
        public RoomInfo GetRoomInfo()
        {
            return new RoomInfo()
            {
                name = Name,
                creator = Creator ?? "",
                environment = EnvironmentID,
                hasPassword = !string.IsNullOrEmpty(Password),
                isHibernating = Hibernating,
                lastInteractionTime = LastInteractionDateTime,
                visitors = new List<string>(Visitors)
            };
        }

        /// <summary>
        /// Update the state of the Room and its simulation
        /// </summary>
        /// <param name="dt">Delta time between updates</param>
        public void Update(float dt)
        {
            if (Hibernating) return;

            dt *= TimeMultiplier;

            // Check if too much time has passed
            if ((Environment.TickCount64 - LastInteractionTime) / 1000f > Timeout)
            {
                // Go to sleep
                Hibernating = true;

                // Kick all users
                while (activeSockets.Count > 0)
                {
                    RemoveSocket(activeSockets[0]);
                }

                return;
            }

            OnUpdate?.Invoke(this, dt);

            SimInstance.Update(dt);
        }

        public static Room Create(string name, string password, string environment, string creator, string roomNamespace = "anonymous", bool startHibernating = false)
        {
            // Verify we have capacity
            if (Program.Rooms.Count(r => !r.Value.Hibernating) >= SettingsManager.MaxRooms)
            {
                throw new Exception("Failed to create room: insufficient resources");
            }

            var newRoom = new Room(name, password, environment);

            newRoom.Name += "@" + roomNamespace;
            newRoom.Creator = creator;
            newRoom.Visitors.Add(creator);
            newRoom.Hibernating = startHibernating;

            Program.Rooms[newRoom.Name] = newRoom;
            newRoom.SendAPIUpdate();

            return newRoom;
        }

        private void SendAPIUpdate()
        {
            try
            {
                HttpClient client = new()
                {
                    BaseAddress = new Uri(SettingsManager.MainAPIServer)
                };

                var request = new HttpRequestMessage(HttpMethod.Patch, "/server/rooms")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "rooms", JsonSerializer.Serialize(new List<RoomInfo>() { GetRoomInfo() }, new JsonSerializerOptions() { IncludeFields = true }) } })
                };

                client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not announce to main API server: " + ex);
            }
        }
    }
}