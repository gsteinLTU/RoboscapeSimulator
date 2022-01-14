namespace IoTScape
{
    [Serializable]
    public class IoTScapeServiceDefinition
    {
        public string name;
        public IoTScapeServiceDescription service;
        public string id;
        public Dictionary<string, IoTScapeMethodDescription> methods = new();
        public Dictionary<string, IoTScapeEventDescription> events = new();

        public IoTScapeServiceDefinition()
        {

        }

        public IoTScapeServiceDefinition(IoTScapeServiceDefinition other)
        {
            name = other.name;
            service = other.service;
            id = other.id;
            methods = other.methods;
            events = other.events;
        }

        public IoTScapeServiceDefinition(string name, IoTScapeServiceDescription service, string id, Dictionary<string, IoTScapeMethodDescription> methods, Dictionary<string, IoTScapeEventDescription> events)
        {
            this.name = name;
            this.service = service;
            this.id = id;
            this.methods = methods;
            this.events = events;
        }
    }
}