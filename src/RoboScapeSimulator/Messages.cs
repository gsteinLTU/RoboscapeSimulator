using System.Diagnostics;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Server.Client;

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
        internal static void SendAvailableRooms(SocketIOSocket socket, IDictionary<string, Room> rooms)
        {
            Utils.sendAsJSON(socket, "availableRooms", new Dictionary<string, object> { { "availableRooms", rooms.Select(room => room.Value.GetRoomInfo()) }, { "canCreate", rooms.Count(r => !r.Value.Hibernating) < SettingsManager.MaxRooms } });
            Utils.sendAsJSON(socket, "availableEnvironments", Room.ListEnvironments());
        }

        /// <summary>
        /// Send the rooms created  and environments to a socket
        /// </summary>
        internal static void SendUserRooms(SocketIOSocket socket, string user, IDictionary<string, Room> rooms)
        {
            SendAvailableRooms(socket, rooms.Where(pair => pair.Value.Creator == user).ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        /// <summary>
        /// Send update on room's status to user
        /// </summary>
        internal static void SendUpdate(SocketIOSocket socket, Room room, bool isFullUpdate = false)
        {
            Dictionary<string, object> updateData = room.SimInstance.GetBodies(!isFullUpdate, isFullUpdate);
            updateData.Add("time", room.SimInstance.Time);

            Utils.sendAsJSON(socket, isFullUpdate ? "fullUpdate" : "u", updateData);
        }

        internal static void HandleJoinRoom(JToken[] args, SocketIOSocket socket, IDictionary<string, Room> rooms, ref string socketRoom)
        {
            // Remove from existing room
            if (!string.IsNullOrWhiteSpace(socketRoom))
            {
                rooms[socketRoom].RemoveSocket(socket);
            }

            // Create room if requested
            string roomID = (string)args[0]["roomID"];
            if (roomID == "create")
            {
                // Verify we have capacity
                if (rooms.Count(r => !r.Value.Hibernating) >= SettingsManager.MaxRooms)
                {
                    socket.Emit("error", "Failed to create room: insufficient resources");
                    return;
                }

                Room newRoom = new("", (string)args[0]["password"] ?? "", (string)args[0]["env"] ?? "default");

                if ((string)args[0]["namespace"] != null)
                {
                    newRoom.Name += "@" + (string)args[0]["namespace"];

                    // For current NetsBlox implementation, namespace is username of creating user
                    newRoom.Creator = (string)args[0]["namespace"];
                }

                rooms[newRoom.Name] = newRoom;
                socketRoom = newRoom.Name;
            }
            else
            {
                // Joining existing room, make sure it exists first
                if (rooms.ContainsKey(roomID))
                {
                    if (rooms[roomID].Password == "" || rooms[roomID].Password == (string)args[0]["password"])
                    {
                        rooms[roomID].Hibernating = false;
                        socketRoom = (string)args[0]["roomID"];
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(socketRoom))
            {
                // Setup updates for socket in new room 
                rooms[socketRoom].AddSocket(socket);
                Utils.sendAsJSON(socket, "roomJoined", socketRoom);
                Utils.sendAsJSON(socket, "roomInfo", rooms[socketRoom].GetInfo());
                SendUpdate(socket, rooms[socketRoom], true);
            }
            else
            {
                // Join failed
                Utils.sendAsJSON(socket, "roomJoined", false);
                Trace.WriteLine("Failed attempt to join room " + roomID);
            }
        }
    }
}