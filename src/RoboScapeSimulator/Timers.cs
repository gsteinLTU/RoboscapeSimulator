using System.Collections.Concurrent;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoboScapeSimulator
{
    /// <summary>
    /// Helper functions to create timers used in main server code
    /// </summary>
    internal static class Timers
    {
        public static System.Timers.Timer CreateClientUpdateTimer(int updateFPS, IDictionary<string, Room> rooms)
        {
            var clientUpdateTimer = new System.Timers.Timer(1000d / updateFPS);

            clientUpdateTimer.Elapsed += (source, e) =>
            {
                foreach (Room room in rooms.Values)
                {
                    if (room.SkipNextUpdate)
                    {
                        room.SkipNextUpdate = false;
                        continue;
                    }

                    lock (room.activeSockets)
                    {
                        foreach (var socket in room.activeSockets)
                        {
                            Messages.SendUpdate(socket, room);
                        }
                    }
                }

            };

            clientUpdateTimer.Start();
            return clientUpdateTimer;
        }

        public static System.Timers.Timer CreateClientFullUpdateTimer(IDictionary<string, Room> rooms, JsonSerializer serializer)
        {
            var clientFullUpdateTimer = new System.Timers.Timer(60000d);

            clientFullUpdateTimer.Elapsed += (source, e) =>
            {
                foreach (Room room in rooms.Values)
                {
                    using var writer = new JTokenWriter();
                    serializer.Serialize(writer, room.SimInstance.GetBodies());
                    foreach (var socket in room.activeSockets)
                    {
                        Messages.SendUpdate(socket, room, true);
                    }
                }
            };

            clientFullUpdateTimer.Start();

            return clientFullUpdateTimer;
        }

        public static System.Timers.Timer CreateCleanDeadRoomsTimer(ConcurrentDictionary<string, Room> rooms)
        {
            var cleanDeadRoomsTimer = new System.Timers.Timer(600000d);

            cleanDeadRoomsTimer.Elapsed += (source, e) =>
            {
                // If room is Hibernating and past its TTL, remove it
                var oldRooms = rooms.Where(pair => pair.Value.Hibernating && (DateTime.Now - pair.Value.LastInteractionTime).TotalSeconds > pair.Value.MaxHibernateTime).ToList();

                if (oldRooms.Count > 0)
                {
                    Trace.WriteLine($"Removing {oldRooms.Count} old rooms");
                    oldRooms.ForEach(pair => rooms.TryRemove(pair));
                }
            };

            cleanDeadRoomsTimer.Start();

            return cleanDeadRoomsTimer;
        }
    }
}