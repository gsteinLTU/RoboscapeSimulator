using System.Diagnostics;
using System.Text.Json;

namespace RoboScapeSimulator
{
    /// <summary>
    /// Helper functions to create timers used in main server code
    /// </summary>
    internal static class Timers
    {
        /// <summary>
        /// Create the timer that updates the rooms' simulations
        /// </summary>
        /// <param name="simFPS">Rate to attempt to update rooms at</param>
        /// <returns>Created Timer object</returns>
        public static Timer CreateRoomUpdateTimer(double simFPS)
        {
            var fpsSpan = TimeSpan.FromSeconds(1d / simFPS);
            long lastTicks = Environment.TickCount64 - (long)fpsSpan.TotalMilliseconds;
            var updateTimer = new Timer((e) =>
            {
                lock (Program.Rooms)
                {
                    foreach (Room room in Program.Rooms.Values)
                    {
                        room.Update((Environment.TickCount64 - lastTicks) / 1000f);
                    }
                }

                Program.IoTScapeManager.Update((Environment.TickCount64 - lastTicks) / 1000f);

                lastTicks = Environment.TickCount64;
            });
            updateTimer.Change(fpsSpan, fpsSpan);
            return updateTimer;
        }

        /// <summary>
        /// Create the timer which sends updates to the clients
        /// </summary>
        /// <param name="updateFPS">Rate (per second) to update clients at</param>
        /// <param name="updatesUntilFullUpdate">Number of incremental updates to send before sending a new full Update</param>
        /// <returns>Created Timer object</returns>
        public static Timer CreateClientUpdateTimer(double updateFPS, double updatesUntilFullUpdate = 2400)
        {
            TimeSpan period = TimeSpan.FromMilliseconds(1000d / updateFPS);

            int i = 0;
            var updateTimer = new Timer((e) =>
            {
                bool isFullUpdate = false;
                if (i++ > updatesUntilFullUpdate)
                {
                    i = 0;
                    isFullUpdate = true;
                }

                lock (Program.Rooms)
                {
                    Program.Rooms.AsParallel().ForAll(kvp =>
                    {
                        var room = kvp.Value;
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

        /// <summary>
        /// Create the Timer watching for very old hibernating rooms to remove
        /// </summary>     
        /// <returns>Created Timer object</returns>
        public static Timer CreateCleanDeadRoomsTimer()
        {
            TimeSpan period = TimeSpan.FromSeconds(60 * 10);
            var deadRoomTimer = new Timer((e) =>
            {
                lock (Program.Rooms)
                {
                    Trace.WriteLine($"{Program.Rooms.Count} rooms, {Program.Rooms.Count(room => !room.Value.Hibernating)} not hibernating");

                    // If room is Hibernating and past its TTL, remove it 
                    var oldRooms = Program.Rooms.Where(pair => pair.Value.Hibernating && (Environment.TickCount64 - pair.Value.LastInteractionTime) / 1000f > pair.Value.MaxHibernateTime).ToList();

                    if (oldRooms.Count > 0)
                    {
                        Trace.WriteLine($"Removing {oldRooms.Count} old rooms");
                        oldRooms.ForEach(pair => Program.Rooms.TryRemove(pair));

                        // Send delete message to main API server
                        try
                        {
                            HttpClient client = new()
                            {
                                BaseAddress = new Uri(SettingsManager.MainAPIServer)
                            };

                            var request = new HttpRequestMessage(HttpMethod.Delete, "/server/rooms")
                            {
                                Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "rooms", JsonSerializer.Serialize(oldRooms.Select(kvp => kvp.Value.GetRoomInfo()), new JsonSerializerOptions() { IncludeFields = true }) } })
                            };

                            client.SendAsync(request);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Could not announce to main API server: " + ex);
                        }
                    }

                }
            });

            deadRoomTimer.Change(period, period);

            return deadRoomTimer;
        }

        /// <summary>
        /// Create the Timer for sending updates to the API server
        /// </summary>
        /// <returns>Created Timer object</returns>
        public static Timer CreateMainAPIServerAnnounceTimer()
        {
            TimeSpan period = TimeSpan.FromSeconds(5 * 60);

            static async void apiAnnounce(object? e)
            {
                try
                {
                    HttpClient client = new()
                    {
                        BaseAddress = new Uri(SettingsManager.MainAPIServer),
                        Timeout = TimeSpan.FromSeconds(15)
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, "/server/announce")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "maxRooms", SettingsManager.MaxRooms.ToString() } })
                    };

                    await client.SendAsync(request);

                    // Send environments list as well
                    request = new HttpRequestMessage(HttpMethod.Post, "/server/environments")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "environments", JsonSerializer.Serialize(Room.ListEnvironments()) } })
                    };

                    _ = client.SendAsync(request);

                    // Send environments list as well
                    request = new HttpRequestMessage(HttpMethod.Put, "/server/rooms")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "rooms", JsonSerializer.Serialize(Program.Rooms.Values.Select(room => room.GetRoomInfo())) } })
                    };

                    _ = client.SendAsync(request);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not announce to main API server: " + ex);
                }
            }

            var apiAnnounceTimer = new Timer(apiAnnounce);

            apiAnnounceTimer.Change(period, period);

            apiAnnounce(null);

            return apiAnnounceTimer;
        }
    }
}