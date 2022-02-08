using RoboScapeSimulator;
using Xunit;
namespace RoboScapeSimulatorTests;

public class RoomTests
{
    [Fact]
    public void CreateRoom()
    {
        Room testRoom = new Room("testRoom");
        Assert.Equal("testRoom", testRoom.Name);
    }
}