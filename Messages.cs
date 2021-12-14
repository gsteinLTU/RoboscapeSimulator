
using SocketIOSharp.Server.Client;

internal static class Messages
{
    /// <summary>
    /// Send the available rooms and environments to a socket
    /// </summary>
    internal static void SendAvailableRooms(SocketIOSocket socket, IDictionary<string, Room> rooms)
    {
        Utils.sendAsJSON(socket, "availableRooms", new Dictionary<string, object> { { "availableRooms", rooms.Keys }, { "canCreate", rooms.Count(r => !r.Value.Hibernating) < SettingsManager.MaxRooms } });
        Utils.sendAsJSON(socket, "availableEnvironments", Room.ListEnvironments());
    }

    /// <summary>
    /// Send the rooms created  and environments to a socket
    /// </summary>
    internal static void SendUserRooms(SocketIOSocket socket, string user, IDictionary<string, Room> rooms)
    {
        var userRooms = rooms.Where(pair => pair.Value.Creator == user).Select(pair => pair.Key).ToList();
        Utils.sendAsJSON(socket, "availableRooms", new Dictionary<string, object> { { "availableRooms", userRooms }, { "canCreate", rooms.Count(r => !r.Value.Hibernating) < SettingsManager.MaxRooms } });
        Utils.sendAsJSON(socket, "availableEnvironments", Room.ListEnvironments());
    }

    /// <summary>
    /// Send update on room's status to user
    /// </summary>
    internal static void SendUpdate(SocketIOSocket socket, Room room, bool isFullUpdate = false)
    {
        Dictionary<string, object> updateData = room.SimInstance.GetBodies(!isFullUpdate);
        updateData.Add("time", room.SimInstance.Time);

        Utils.sendAsJSON(socket, isFullUpdate ? "fullUpdate" : "update", updateData);
    }
}