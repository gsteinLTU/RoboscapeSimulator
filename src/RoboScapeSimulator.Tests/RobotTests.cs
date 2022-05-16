using System.Linq;
using System.Numerics;
using RoboScapeSimulator.Entities.Robots;
using Xunit;
namespace RoboScapeSimulator.Tests;

public class RobotTests
{
    [Fact]
    public void CreateRobot()
    {
        Room testRoom = new("testroom");

        Robot robot = testRoom.SimInstance.Robots.First();

        wait(testRoom);

        // Did robot go to floor
        Assert.InRange(robot.Position.Y, -0.1f, 0.1f);
    }

    [Fact]
    public void DriveRobot()
    {
        Room testRoom = new("testroom");

        ParallaxRobot robot = (ParallaxRobot)testRoom.SimInstance.Robots.First();

        wait(testRoom);

        robot.MessageHandlers['S'](new byte[] { (byte)'S', 0x64, 0, 0x64, 0 });

        Vector3 initialPosition = new(robot.Position.X, robot.Position.Y, robot.Position.Z);

        wait(testRoom);

        double distance = (robot.Position - initialPosition).Length();

        // Did robot go forward
        Assert.InRange(distance, 0.5, double.PositiveInfinity);

        // Did ticks go up
        Assert.InRange(robot.LeftTicks, 1d, double.PositiveInfinity);
        Assert.InRange(robot.RightTicks, 1d, double.PositiveInfinity);
    }

    [Fact]
    public void ResetRobot()
    {
        Room testRoom = new("testroom");

        ParallaxRobot robot = (ParallaxRobot)testRoom.SimInstance.Robots.First();
        wait(testRoom);

        // Send set speed 100 100
        robot.MessageHandlers['S'](new byte[] { (byte)'S', 0x64, 0, 0x64, 0 });

        Vector3 initialPosition = new(robot.Position.X, robot.Position.Y, robot.Position.Z);

        wait(testRoom);

        // Did robot move
        double distance = (robot.Position - initialPosition).Length();
        Assert.InRange(distance, 0.2, double.PositiveInfinity);

        // Did ticks go up
        Assert.InRange(robot.LeftTicks, 1d, double.PositiveInfinity);
        Assert.InRange(robot.RightTicks, 1d, double.PositiveInfinity);

        testRoom.ResetRobot(robot.ID, "");

        wait(testRoom);

        distance = (robot.Position - initialPosition).Length();

        // Did robot go back
        Assert.InRange(distance, 0, 0.07);

        // Did ticks go back to zero
        Assert.Equal(0, robot.LeftTicks);
        Assert.Equal(0, robot.RightTicks);
    }


    [Fact]
    public void ResetRobotAndDrive()
    {
        Room testRoom = new("testroom");

        ParallaxRobot robot = (ParallaxRobot)testRoom.SimInstance.Robots.First();

        wait(testRoom);

        Vector3 initialPosition = new(robot.Position.X, robot.Position.Y, robot.Position.Z);

        // Send set speed 100 100
        robot.MessageHandlers['S'](new byte[] { (byte)'S', 0x64, 0, 0x64, 0 });

        wait(testRoom);

        // Did robot move
        double distance = (robot.Position - initialPosition).Length();
        Assert.InRange(distance, 0.2, double.PositiveInfinity);

        // Did ticks go up
        Assert.InRange(robot.LeftTicks, 1d, double.PositiveInfinity);
        Assert.InRange(robot.RightTicks, 1d, double.PositiveInfinity);

        testRoom.ResetRobot(robot.ID, "");

        wait(testRoom);

        distance = (robot.Position - initialPosition).Length();

        // Did robot go back
        Assert.InRange(distance, 0, 0.05);

        // Did ticks go back to zero
        Assert.Equal(0, robot.LeftTicks);
        Assert.Equal(0, robot.RightTicks);

        // Send set speed 100 100
        robot.MessageHandlers['S'](new byte[] { (byte)'S', 0x64, 0, 0x64, 0 });

        wait(testRoom);

        distance = (robot.Position - initialPosition).Length();

        // Did robot move
        Assert.InRange(distance, 0.1, double.PositiveInfinity);

        // Did ticks go up
        Assert.InRange(robot.LeftTicks, 1d, double.PositiveInfinity);
        Assert.InRange(robot.RightTicks, 1d, double.PositiveInfinity);
    }

    private static void wait(Room testRoom, int ticks = 250, int fps = 30)
    {
        for (int i = 0; i < ticks; i++)
        {
            testRoom.Update(1.0f / fps);
        }
    }
}