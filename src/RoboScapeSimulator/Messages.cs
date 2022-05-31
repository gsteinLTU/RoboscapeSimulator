using System.Diagnostics;
using System.Text.Json.Nodes;

namespace RoboScapeSimulator
{
    /// <summary>
    /// Static helper functions for various messages sent to/from the client
    /// </summary>
    internal static class Messages
    {
        /// <summary>
        /// Send the available rooms and environments to a socket
        /// </summary>
        internal static void SendAvailableRooms(Node.Socket socket, IDictionary<string, Room> rooms)
        {
            Utils.SendAsJSON(socket, "availableRooms", new Dictionary<string, object> { { "availableRooms", rooms.Select(room => room.Value.GetRoomInfo()) }, { "canCreate", rooms.Count(r => !r.Value.Hibernating) < SettingsManager.MaxRooms } });
            Utils.SendAsJSON(socket, "availableEnvironments", Room.ListEnvironments());
        }

        /// <summary>
        /// Send the rooms created  and environments to a socket
        /// </summary>
        internal static void SendUserRooms(Node.Socket socket, string user, IDictionary<string, Room> rooms)
        {
            SendAvailableRooms(socket, rooms.Where(pair => pair.Value.Visitors.Contains(user)).ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        /// <summary>
        /// Send update on room's status to user
        /// </summary>
        internal static void SendUpdate(Node.Socket socket, Room room, bool isFullUpdate = false)
        {
            Dictionary<string, object> updateData = room.SimInstance.GetBodies(!isFullUpdate, isFullUpdate);
            updateData.Add("time", room.SimInstance.Time);

            Utils.SendAsJSON(socket, isFullUpdate ? "fullUpdate" : "u", updateData);
        }

        internal static void HandleJoinRoom(JsonNode[] args, Node.Socket socket, IDictionary<string, Room> rooms, ref string socketRoom)
        {
            // Remove from existing room
            if (!string.IsNullOrWhiteSpace(socketRoom))
            {
                rooms[socketRoom].RemoveSocket(socket);
            }

            if (args.Length == 0 || args[0]["roomID"] == null)
            {
                // Invalid message
                Utils.SendAsJSON(socket, "roomJoined", false);
                Trace.WriteLine("Failed attempt to join room");
                return;
            }

            // Create room if requested
            var roomID = args[0]["roomID"]?.ToString();
            if (roomID == "create")
            {
                // Verify we have capacity
                if (rooms.Count(r => !r.Value.Hibernating) >= SettingsManager.MaxRooms)
                {
                    socket.Emit("error", "Failed to create room: insufficient resources");
                    return;
                }

                Room newRoom = new("", args[0]["password"]?.ToString() ?? "", args[0]["env"]?.ToString() ?? "default");

                string? roomNamespace = (args[0]["namespace"] ?? args[0]["username"])?.ToString();
                if (roomNamespace != null)
                {
                    newRoom.Name += "@" + roomNamespace;

                    // For current NetsBlox implementation, namespace is username of creating user
                    newRoom.Creator = roomNamespace;
                }

                rooms[newRoom.Name] = newRoom;
                socketRoom = newRoom.Name;
            }
            else
            {
                // Joining existing room, make sure it exists first
                if (roomID != null && rooms.ContainsKey(roomID))
                {
                    if (rooms[roomID].Password == "" || rooms[roomID].Password == args[0]["password"]?.ToString())
                    {
                        rooms[roomID].Hibernating = false;
                        socketRoom = roomID;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(socketRoom))
            {
                // Setup updates for socket in new room 
                rooms[socketRoom].AddSocket(socket, args[0]["username"]?.ToString());
                Utils.SendAsJSON(socket, "roomJoined", socketRoom);
                Utils.SendAsJSON(socket, "roomInfo", rooms[socketRoom].GetInfo());
                SendUpdate(socket, rooms[socketRoom], true);
            }
            else
            {
                // Join failed
                Utils.SendAsJSON(socket, "roomJoined", false);
                Trace.WriteLine("Failed attempt to join room " + roomID);
            }
        }
    }
}