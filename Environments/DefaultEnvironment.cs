class DefaultEnvironment : EnvironmentConfiguration
{
    public DefaultEnvironment()
    {
        Name = "Default";
        ID = "default";
        Description = "The default environment";
    }

    public override void Setup(Room room)
    {
        Console.WriteLine("Setting up default environment");

        // Ground
        var ground = new Ground(room);

        // Demo robot
        var robot = new ParallaxRobot(room, debug: false);

        for (int i = 0; i < 3; i++)
        {
            var cube = new Cube(room, visualInfo: "#B85");
        }
    }
}