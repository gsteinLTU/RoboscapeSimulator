using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace IoTScape
{
    public class IoTScapeManager
    {
        public static IoTScapeManager? Manager;

        private IPAddress hostIpAddress;

        private Socket _socket;

        private int idprefix;

        private ConcurrentDictionary<string, IoTScapeObject> objects = new();
        private ConcurrentDictionary<string, int> lastIDs = new();

        private EndPoint hostEndPoint;
        // Wait time in seconds.
        private float waitTime = 30.0f;
        private float timer = 0.0f;

        public IoTScapeManager()
        {
            hostIpAddress = Dns.GetHostAddresses(SettingsManager.RoboScapeHostWithoutPort)[0];
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
        void announce(IoTScapeObject o)
        {
            string serviceJson = JsonConvert.SerializeObject(new Dictionary<string, IoTScapeServiceDefinition>() { { o.Definition.name, o.Definition } });
            Console.WriteLine($"Announcing service {o.Definition.name} from object with ID {o.Definition.id}");

            _socket.SendTo(serviceJson.Select(c => (byte)c).ToArray(), SocketFlags.None, hostEndPoint);
        }

        /// <summary>
        /// Announce all object-services to server
        /// </summary>
        void announceAll()
        {
            objects.Values.ToList().ForEach(announce);
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
                Console.WriteLine("IoTScapeObject " + o.Definition.name + ":" + o.Definition.id + " already registered.");
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
            announce(o);

            return o.Definition.id;
        }

        /// <summary>
        /// Unregister an IoTScapeObject
        /// </summary>
        /// <param name="o">IoTScapeObject to register</param>
        /// <returns>ID of IoTScapeObject</returns>
        public void Unregister(IoTScapeObject o)
        {
            if (IsRegistered(o))
            {
                objects.TryRemove(o.Definition.name + ":" + o.Definition.id, out var _);
            }
        }

        public bool IsRegistered(IoTScapeObject o)
        {
            return objects.ContainsKey(o.Definition.name + ":" + o.Definition.id);
        }

        // Update is called once per frame
        public void Update(float dt)
        {
            // Parse incoming messages
            if (_socket.Available > 0)
            {
                byte[] incoming = new byte[2048];
                int len = _socket.Receive(incoming);

                string incomingString = Encoding.UTF8.GetString(incoming, 0, len);

                var json = JsonSerializer.Create();
                IoTScapeRequest request = json.Deserialize<IoTScapeRequest>(new JsonTextReader(new StringReader(incomingString)));
                Console.WriteLine(request);

                // Verify device exists
                if (objects.ContainsKey(request.service + ":" + request.device))
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

            if (timer > waitTime)
            {
                announceAll();
                timer = 0.0f;
            }
        }

        /// <summary>
        /// Generate and send a response with a result
        /// </summary>
        /// <param name="request">Request to respond to</param>
        /// <param name="result">Result to send to server for request's response</param>
        private void SendResponse(IoTScapeRequest request, string[] result)
        {
            IoTScapeResponse response = new IoTScapeResponse
            {
                id = request.device,
                request = request.id,
                service = request.service,
                response = (result ?? new string[] { }).ToList()
            };

            SendToServer(response);
        }

        /// <summary>
        /// Send an IoTScapeResponse object to the server as JSON
        /// </summary>
        /// <param name="response">Response to send to server</param>
        internal void SendToServer(IoTScapeResponse response)
        {
            // Send response
            string responseJson = JsonConvert.SerializeObject(response,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            Console.WriteLine(responseJson);

            _socket.SendTo(responseJson.Select(c => (byte)c).ToArray(), SocketFlags.None, hostEndPoint);
        }
    }

    [Serializable]
    public class IoTScapeEventDescription
    {
        [JsonProperty(PropertyName = "params")]
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
        public string? documentation;

        [JsonProperty(PropertyName = "params")]
        public List<IoTScapeMethodParams> paramsList = new();
        public IoTScapeMethodReturns returns = new();
    }

    [Serializable]
    public class IoTScapeMethodParams
    {
        public string name = "param";
        public string? documentation;
        public string type = "string";
        public bool optional;
    }

    [Serializable]
    public class IoTScapeMethodReturns
    {
        public string? documentation;
        public List<string> type = new();
    }

    [Serializable]
    public class IoTScapeRequest
    {
        public string id = "";
        public string service = "";
        public string device = "";
        public string? function;

        [JsonProperty(PropertyName = "params")]
        public List<string> ParamsList = new();

        public override string ToString()
        {
            return $"IoTScape Request #{id}: call {service}/{function} on {device} with params [{string.Join(", ", ParamsList)}]";
        }
    }

    [Serializable]
    public class IoTScapeResponse
    {
        public string id = "";
        public string request = "";
        public string service = "";
        public List<string>? response;

        [JsonProperty(PropertyName = "event")]
        public IoTScapeEventResponse? EventResponse;

        public string? error;
    }

    [Serializable]
    public class IoTScapeEventResponse
    {
        public string? type;
        public string[]? args;
    }
}