/// <summary>
/// Base class for environment types
/// </summary>
abstract class EnvironmentConfiguration
{
    /// <summary>
    /// Displayed name of this environment
    /// </summary>
    public static string Name;

    /// <summary>
    /// Identifying value for this environment
    /// </summary>
    public static string ID;

    /// <summary>
    /// Descriptive text for this environment
    /// </summary>
    public static string Description;

    /// <summary>
    /// Configures <paramref name="sim"/> to run this environment
    /// </summary>
    /// <param name="sim">SimulationInstance to apply changes to</param>
    public static void Setup(SimulationInstance sim) { }
}