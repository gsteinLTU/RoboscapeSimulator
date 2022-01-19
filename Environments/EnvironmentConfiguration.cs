/// <summary>
/// Base class for environment types
/// </summary>
abstract class EnvironmentConfiguration
{
    /// <summary>
    /// Displayed name of this environment
    /// </summary>
    public string Name = "";

    /// <summary>
    /// Identifying value for this environment
    /// </summary>
    public string ID = "";

    /// <summary>
    /// Descriptive text for this environment
    /// </summary>
    public string Description = "";

    /// <summary>
    /// Configures <paramref name="room"/> to run this environment
    /// </summary>
    /// <param name="room">Room to apply changes to</param>
    public virtual void Setup(Room room) { }
}