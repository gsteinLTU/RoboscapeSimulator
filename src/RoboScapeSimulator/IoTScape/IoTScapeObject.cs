using IoTScapeCommandCallback = System.Func<string[], string[]>;

namespace RoboScapeSimulator.IoTScape
{
    public class IoTScapeObject
    {
        public IoTScapeServiceDefinition Definition;

        public string DeviceTypeID = "";

        public string IDOverride = "";

        public Dictionary<string, IoTScapeCommandCallback> Methods = new();

        internal string ID = "";

        public bool ShouldRegister = true;

        internal Room? _room;

        // Start is called before the first frame update
        public IoTScapeObject(IoTScapeServiceDefinition definition, string? id = null)
        {
            Definition = new IoTScapeServiceDefinition(definition)
            {
                id = id ?? Guid.NewGuid().ToString()[^6..]
            };

            // Populate Methods list
            foreach (var method in Definition.methods.Keys)
            {
                if (!Methods.ContainsKey(method))
                {
                    Methods.Add(method, (string[] input) => Array.Empty<string>());
                }
            }

            // Register default heartbeat
            Methods["heartbeat"] = Heartbeat;
        }

        /// <summary>
        /// Respond to the server with a heartbeat
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string[] Heartbeat(string[] args)
        {
            return new[] { "true" };
        }

        /// <summary>
        /// Send an event for this device to the server
        /// </summary>
        /// <param name="eventType">Type of event, should be in service definition</param>
        /// <param name="args">Data to be passed with event</param>
        public void SendEvent(string eventType, string[] args)
        {
            IoTScapeResponse eventResponse = new()
            {
                id = ID,
                service = Definition.name,
                request = "",
                EventResponse = new IoTScapeEventResponse()
                {
                    type = eventType,
                    args = args
                }
            };

            IoTScapeManager.Manager?.SendToServer(eventResponse);
        }

        /// <summary>
        /// Send an event with no arguments
        /// </summary>
        /// <param name="EventType">Type of event, should be in service definition</param>
        public void SendEvent(string EventType)
        {
            SendEvent(EventType, Array.Empty<string>());
        }

        public void Setup(Room room)
        {
            _room = room;

            IoTScapeManager.Manager?.Register(this);

            room.OnHibernateStart += (sender, e) =>
            {
                IoTScapeManager.Manager?.Unregister(this);
            };

            room.OnHibernateEnd += (sender, e) =>
            {
                IoTScapeManager.Manager?.Register(this);
            };

            room.OnRoomClose += (sender, e) =>
            {
                IoTScapeManager.Manager?.Unregister(this);
            };
        }
    }


}