using Microsoft.Extensions.Configuration;

/// <summary>
/// Stores settings for the program in general
/// </summary>
class SettingsManager
{
    /// <summary>
    /// The NetsBlox server to connect to
    /// </summary>
    public static string RoboScapeHostWithoutPort
    {
        get
        {
            if (loadedSettings == null)
            {
                loadSettings();
            }

            if (loadedSettings?.RoboScapeHost == null)
            {
                return "editor.netsblox.org";
            }


            return loadedSettings.RoboScapeHost;
        }
    }

    /// <summary>
    /// The RoboScape port for the given server
    /// </summary>
    public static int RoboScapePort
    {
        get
        {
            if (loadedSettings == null)
            {
                loadSettings();
            }

            if (loadedSettings?.RoboScapePort <= 0)
            {
                return 1973;
            }

            return loadedSettings.RoboScapePort;
        }
    }

    static RoboScapeSimSettings loadedSettings;

    /// <summary>
    /// Loads the configuration in appsettings.json
    /// </summary>
    private static void loadSettings()
    {
        var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

        var section = config.GetSection(nameof(RoboScapeSimSettings));
        loadedSettings = section.Get<RoboScapeSimSettings>();
    }

}

/// <summary>
/// Settings data for the program, as stored in appsettings.json
/// </summary>
public class RoboScapeSimSettings
{
    public string RoboScapeHost { get; set; }
    public int RoboScapePort { get; set; }
}