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
        public static Timer CreateClientUpdateTimer(double updateFPS, IDictionary<string, Room> rooms, bool isFullUpdate = false)
        {
            TimeSpan period = TimeSpan.FromMilliseconds(1000d / updateFPS);

            var updateTimer = new Timer((e) =>
            {
                lock (rooms)
                {
                    rooms.AsParallel().ForAll(kvp =>
                    {
                        var room = kvp.Value;
                        if (!isFullUpdate && room.SkipNextUpdate)
                        {
                            room.SkipNextUpdate = false;
                            return;
                        }

                        lock (room.activeSockets)
                        {
                            foreach (var socket in room.activeSockets)
                            {
                                Messages.SendUpdate(socket, room, isFullUpdate);
                            }
                        }
                    });
                }

            });
            updateTimer.Change(period, period);
            return updateTimer;
        }

        public static Timer CreateClientFullUpdateTimer(IDictionary<string, Room> rooms, JsonSerializer serializer)
        {
            return CreateClientUpdateTimer(1d / 60d, rooms, true);
        }

        public static Timer CreateCleanDeadRoomsTimer(ConcurrentDictionary<string, Room> rooms)
        {
            TimeSpan period = TimeSpan.FromSeconds(60 * 10);
            var deadRoomTimer = new Timer((e) =>
            {
                lock (rooms)
                {
                    // If room is Hibernating and past its TTL, remove it
                    var oldRooms = rooms.Where(pair => pair.Value.Hibernating && (DateTime.Now - pair.Value.LastInteractionTime).TotalSeconds > pair.Value.MaxHibernateTime).ToList();

                    if (oldRooms.Count > 0)
                    {
                        Trace.WriteLine($"Removing {oldRooms.Count} old rooms");
                        oldRooms.ForEach(pair => rooms.TryRemove(pair));
                    }
                }
            });

            deadRoomTimer.Change(period, period);

            return deadRoomTimer;
        }
    }
}