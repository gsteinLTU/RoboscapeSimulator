using System.Diagnostics;

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
                    }

                }
            });

            deadRoomTimer.Change(period, period);

            return deadRoomTimer;
        }

        public static Timer CreateMainAPIServerAnnounceTimer()
        {
            TimeSpan period = TimeSpan.FromSeconds(5 * 60 * 10);

            static void apiAnnounce(object? e)
            {
                HttpClient client = new();

                var request = new HttpRequestMessage(HttpMethod.Post, SettingsManager.MainAPIServer + "/server/announce")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "maxRooms", SettingsManager.MaxRooms.ToString() } })
                };

                client.SendAsync(request);
            }

            var apiAnnounceTimer = new Timer(apiAnnounce);

            apiAnnounceTimer.Change(period, period);

            apiAnnounce(null);

            return apiAnnounceTimer;
        }
    }
}