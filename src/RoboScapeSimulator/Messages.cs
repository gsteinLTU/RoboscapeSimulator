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
        internal static void SendUserRooms(Node.Socket socket, string user)
        {
            SendAvailableRooms(socket, Program.Rooms.Where(pair => pair.Value.Visitors.Contains(user)).ToDictionary(pair => pair.Key, pair => pair.Value));
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

        internal static void HandleJoinRoom(JsonNode[] args, Node.Socket socket, ref string socketRoom)
        {
            // Remove from existing room
            if (!string.IsNullOrWhiteSpace(socketRoom))
            {
                Program.Rooms[socketRoom].RemoveSocket(socket);
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
                string roomNamespace = (args[0]["namespace"] ?? args[0]["username"])?.ToString() ?? "anonymous";

                try
                {
                    var newRoom = Room.Create("", args[0]["password"]?.ToString() ?? "", args[0]["env"]?.ToString() ?? "default", roomNamespace, roomNamespace);
                    socketRoom = newRoom.Name;
                }
                catch (Exception e)
                {
                    socket.Emit("error", e.Message);
                    return;
                }
            }
            else
            {
                // Joining existing room, make sure it exists first
                if (roomID != null && Program.Rooms.ContainsKey(roomID))
                {
                    if (Program.Rooms[roomID].Password == "" || Program.Rooms[roomID].Password == args[0]["password"]?.ToString())
                    {
                        Program.Rooms[roomID].Hibernating = false;
                        socketRoom = roomID;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(socketRoom))
            {
                // Setup updates for socket in new room 
                Program.Rooms[socketRoom].AddSocket(socket, args[0]["username"]?.ToString());
                Utils.SendAsJSON(socket, "roomJoined", socketRoom);
                Utils.SendAsJSON(socket, "roomInfo", Program.Rooms[socketRoom].GetInfo());
                SendUpdate(socket, Program.Rooms[socketRoom], true);
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