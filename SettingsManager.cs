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

            return (loadedSettings?.RoboScapeHost) ?? DefaultSettings.RoboScapeHost;
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

            return (loadedSettings ?? DefaultSettings).RoboScapePort.GetValueOrDefault((int)DefaultSettings.RoboScapePort);
        }
    }

    /// <summary>
    /// The RoboScape port for the given server
    /// </summary>
    public static int MaxRooms
    {
        get
        {
            if (loadedSettings == null)
            {
                loadSettings();
            }

            return (loadedSettings ?? DefaultSettings).MaxRooms.GetValueOrDefault((int)DefaultSettings.MaxRooms);
        }
    }

    static RoboScapeSimSettings? loadedSettings;

    /// <summary>
    /// Loads the configuration in appsettings.json
    /// </summary>
    private static void loadSettings()
    {
        var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

        var section = config.GetSection(nameof(RoboScapeSimSettings));
        loadedSettings = section.Get<RoboScapeSimSettings>();

        // Validate
        if (loadedSettings == null)
        {
            loadedSettings = DefaultSettings;
        }

        if (string.IsNullOrWhiteSpace(loadedSettings.RoboScapeHost))
        {
            loadedSettings.RoboScapeHost = DefaultSettings.RoboScapeHost;
        }

        if (loadedSettings.RoboScapePort == null || loadedSettings.RoboScapePort <= 0)
        {
            loadedSettings.RoboScapePort = DefaultSettings.RoboScapePort;
        }

        if (loadedSettings.MaxRooms == null)
        {
            loadedSettings.MaxRooms = DefaultSettings.MaxRooms;
        }
    }

    /// <summary>
    /// Default RoboScapeSimSettings to use when none is loaded externally
    /// </summary>
    readonly static RoboScapeSimSettings DefaultSettings = new()
    {
        RoboScapeHost = "editor.netsblox.org",
        RoboScapePort = 1973,
        MaxRooms = 64
    };
}

/// <summary>
/// Settings data for the program, as stored in appsettings.json
/// </summary>
public class RoboScapeSimSettings
{
    public string? RoboScapeHost { get; set; }
    public int? RoboScapePort { get; set; }
    public int? MaxRooms { get; set; }
}