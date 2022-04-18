using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoboScapeSimulator.IoTScape
{
    public class IoTScapeManager
    {
        private static IoTScapeManager? manager;
        public static IoTScapeManager? Manager { get => manager; set => manager = value; }

        private readonly Socket _socket;

        readonly private int idprefix;

        readonly private ConcurrentDictionary<string, IoTScapeObject> objects = new();
        readonly private ConcurrentDictionary<string, int> lastIDs = new();

        readonly private EndPoint hostEndPoint;

        // Wait time in seconds.
        private const float announcePeriod = 30.0f;

        private float timer = 0.0f;

        public IoTScapeManager()
        {
            var hostIpAddress = Dns.GetHostAddresses(SettingsManager.RoboScapeHostWithoutPort)[0];
            hostEndPoint = new IPEndPoint(hostIpAddress, SettingsManager.IoTScapePort);

            idprefix = Random.Shared.Next(0, 0x10000);
            Manager = this;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        /// <summary>
        /// Announces all services to server
        /// </summary>
        /// <param name="o">IoTScapeObject to announce</param>
        void Announce(IoTScapeObject o)
        {
            Debug.WriteLine($"Announcing service {o.Definition.name} from object with ID {o.Definition.id}");
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, IoTScapeServiceDefinition>() { { o.Definition.name, o.Definition } });
            Debug.WriteLine(Encoding.UTF8.GetString(jsonBytes));
            _socket.SendTo(jsonBytes, SocketFlags.None, hostEndPoint);
        }

        /// <summary>
        /// Announce all object-services to server
        /// </summary>
        void AnnounceAll()
        {
            objects.Values.ToList().ForEach(Announce);
        }

        /// <summary>
        /// Register an IoTScapeObject
        /// </summary>
        /// <param name="o">IoTScapeObject to register</param>
        /// <returns>ID of IoTScapeObject</returns>
        public string Register(IoTScapeObject o)
        {
            if (IsRegistered(o))
            {
                Debug.WriteLine("IoTScapeObject " + o.Definition.name + ":" + o.Definition.id + " already registered.");
                return o.Definition.id ?? "";
            }

            int newID;
            string newIDString;
            string deviceIDPrefix = o.IDOverride.Length > 0 ? o.IDOverride : idprefix.ToString("x4");

            if (string.IsNullOrWhiteSpace(o.Definition.id))
            {
                string fullDeviceType = o.Definition.name;

                // Allow custom IDs to have their own numeric ids
                if (o.IDOverride.Length > 0)
                {
                    fullDeviceType += ":" + o.IDOverride;
                }

                // Allow device types to have their own numeric ids
                if (o.DeviceTypeID.Length > 1)
                {
                    fullDeviceType += ":" + o.DeviceTypeID;
                }

                if (!lastIDs.ContainsKey(fullDeviceType))
                {
                    lastIDs.TryAdd(fullDeviceType, 0);
                }

                newID = lastIDs[fullDeviceType]++;

                // Assign IDs
                newIDString = deviceIDPrefix;

                if (o.DeviceTypeID != "")
                {
                    newIDString += "_" + o.DeviceTypeID;
                }

                // Custom ID with no collision skips numeric suffix
                if (!(o.IDOverride.Length > 0 && newID == 0))
                {
                    newIDString += "_" + newID.ToString("x4");
                }

                o.Definition.id = newIDString;
            }

            objects.TryAdd(o.Definition.name + ":" + o.Definition.id, o);
            Announce(o);

            return o.Definition.id;
        }

        /// <summary>
        /// Unregister an IoTScapeObject
        /// </summary>
        /// <param name="o">IoTScapeObject to register</param>
        /// <returns>ID of IoTScapeObject</returns>
        public void Unregister(in IoTScapeObject o)
        {
            if (IsRegistered(o))
            {
                objects.TryRemove(o.Definition.name + ":" + o.Definition.id, out var _);
            }
        }

        public bool IsRegistered(in IoTScapeObject o)
        {
            return objects.ContainsKey(o.Definition.name + ":" + o.Definition.id);
        }

        // Update is called once per frame
        public void Update(in float dt)
        {
            // Parse incoming messages
            if (_socket.Available > 0)
            {
                byte[] incoming = new byte[2048];
                int len = _socket.Receive(incoming);

                string incomingString = Encoding.UTF8.GetString(incoming, 0, len);

                var request = JsonSerializer.Deserialize<IoTScapeRequest>(incomingString);
                Debug.WriteLine(request);

                // Verify device exists
                if (request != null && objects.ContainsKey(request.service + ":" + request.device))
                {
                    var device = objects[request.service + ":" + request.device];

                    // Call function if valid
                    if (request.function != null && device.Methods.ContainsKey(request.function))
                    {
                        string[] result = device.Methods[request.function].Invoke(request.ParamsList.ToArray());


                        // Keep room active if received non-heartbeat IoTScape message
                        if (request.function != "heartbeat" && device._room != null)
                        {
                            device._room.LastInteractionTime = DateTime.Now;
                        }

                        SendResponse(request, result);
                    }
                }
            }

            timer += dt;

            if (timer > announcePeriod)
            {
                AnnounceAll();
                timer = 0.0f;
            }
        }

        /// <summary>
        /// Generate and send a response with a result
        /// </summary>
        /// <param name="request">Request to respond to</param>
        /// <param name="result">Result to send to server for request's response</param>
        private void SendResponse(in IoTScapeRequest request, in string[] result)
        {
            IoTScapeResponse response = new()
            {
                id = request.device,
                request = request.id,
                service = request.service,
                response = (result ?? Array.Empty<string>()).ToList()
            };

            SendToServer(response);
        }

        /// <summary>
        /// Send an IoTScapeResponse object to the server as JSON
        /// </summary>
        /// <param name="response">Response to send to server</param>
        internal void SendToServer(in IoTScapeResponse response)
        {
            // Send response
            _socket.SendTo(JsonSerializer.SerializeToUtf8Bytes(response,
                new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }), SocketFlags.None, hostEndPoint);
        }
    }

    [Serializable]
    public class IoTScapeEventDescription
    {
        [JsonPropertyName("params")]
        public List<string> paramsList = new();
    }

    [Serializable]
    public class IoTScapeServiceDescription
    {
        public string? description;
        public string? externalDocumentation;
        public string? termsOfService;
        public string? contact;
        public string? license;
        public string? version;
    }

    [Serializable]
    public class IoTScapeMethodDescription
    {
        [JsonInclude]
        public string? documentation;

        [JsonInclude]
        [JsonPropertyName("params")]
        public List<IoTScapeMethodParams> paramsList = new();

        [JsonInclude]
        public IoTScapeMethodReturns returns = new();
    }

    [Serializable]
    public class IoTScapeMethodParams
    {

        [JsonInclude]
        public string name = "param";

        [JsonInclude]
        public string? documentation;

        [JsonInclude]
        public string type = "string";

        [JsonInclude]
        public bool optional;
    }

    [Serializable]
    public class IoTScapeMethodReturns
    {
        [JsonInclude]
        public string? documentation;

        [JsonInclude]
        public List<string> type = new();
    }

    [Serializable]
    public class IoTScapeRequest
    {
        [JsonInclude]
        public string id = "";

        [JsonInclude]
        public string service = "";

        [JsonInclude]
        public string device = "";

        [JsonInclude]
        public string? function;

        [JsonInclude]
        [JsonPropertyName("params")]
        public List<string> ParamsList = new();

        public override string ToString()
        {
            return $"IoTScape Request #{id}: call {service}/{function} on {device} with params [{string.Join(", ", ParamsList)}]";
        }
    }

    [Serializable]
    public class IoTScapeResponse
    {
        [JsonInclude]
        public string id = "";

        [JsonInclude]
        public string request = "";

        [JsonInclude]
        public string service = "";

        [JsonInclude]
        public List<string>? response;

        [JsonInclude]
        [JsonPropertyName("event")]
        public IoTScapeEventResponse? EventResponse;

        [JsonInclude]
        public string? error;
    }

    [Serializable]
    public class IoTScapeEventResponse
    {
        [JsonInclude]
        public string? type;

        [JsonInclude]
        public string[]? args;
    }
}