using Xunit;
namespace RoboScapeSimulator.Tests;

public class RoomTests
{
    [Fact]
    public void CreateRoom()
    {
        Room testRoom = new("testRoom");
        Assert.Equal("testRoom", testRoom.Name);
    }

    [Fact]
    public void RoomTimeTest()
    {
        Room testRoom = new("testRoom");
        testRoom.Update(1.0f / 60.0f);
        Assert.InRange(testRoom.Time, 1.0f / 60.0f - float.Epsilon, 1.0f / 60.0f + float.Epsilon);
        testRoom.Update(1.0f / 60.0f);
        Assert.InRange(testRoom.Time, 2.0f / 60.0f - float.Epsilon, 2.0f / 60.0f + float.Epsilon);
    }
}