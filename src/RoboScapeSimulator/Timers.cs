using System.Diagnostics;
using System.Text.Json;
using static RoboScapeSimulator.Room;

namespace RoboScapeSimulator
{
    /// <summary>
    /// Helper functions to create timers used in main server code
    /// </summary>
    internal static class Timers
    {
        public static Timer CreateClientUpdateTimer(double updateFPS, bool isFullUpdate = false)
        {
            TimeSpan period = TimeSpan.FromMilliseconds(1000d / updateFPS);

            var updateTimer = new Timer((e) =>
            {
                lock (Program.Rooms)
                {
                    Program.Rooms.AsParallel().ForAll(kvp =>
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

        public static Timer CreateClientFullUpdateTimer()
        {
            return CreateClientUpdateTimer(1d / 60d, true);
        }

        public static Timer CreateCleanDeadRoomsTimer()
        {
            TimeSpan period = TimeSpan.FromSeconds(60 * 10);
            var deadRoomTimer = new Timer((e) =>
            {
                lock (Program.Rooms)
                {
                    // If room is Hibernating and past its TTL, remove it
                    var oldRooms = Program.Rooms.Where(pair => pair.Value.Hibernating && (DateTime.Now - pair.Value.LastInteractionTime).TotalSeconds > pair.Value.MaxHibernateTime).ToList();

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
                        catch (HttpRequestException)
                        {
                            Trace.WriteLine("Could not announce to main API server");
                        }
                    }

                }
            });

            deadRoomTimer.Change(period, period);

            return deadRoomTimer;
        }

        public static Timer CreateMainAPIServerAnnounceTimer()
        {
            TimeSpan period = TimeSpan.FromSeconds(5 * 60);

            static async void apiAnnounce(object? e)
            {
                try
                {
                    HttpClient client = new()
                    {
                        BaseAddress = new Uri(SettingsManager.MainAPIServer)
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

                    client.SendAsync(request);

                    // Send environments list as well
                    request = new HttpRequestMessage(HttpMethod.Put, "/server/rooms")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "rooms", JsonSerializer.Serialize(Program.Rooms.Values.Select(room => room.GetRoomInfo())) } })
                    };

                    client.SendAsync(request);
                }
                catch (HttpRequestException)
                {
                    Trace.WriteLine("Could not announce to main API server");
                }
            }

            var apiAnnounceTimer = new Timer(apiAnnounce);

            apiAnnounceTimer.Change(period, period);

            apiAnnounce(null);

            return apiAnnounceTimer;
        }
    }
}