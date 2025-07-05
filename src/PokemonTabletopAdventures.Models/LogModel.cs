namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents a Pokemon Tabletop Adventures log
/// </summary>
public class LogModel
{
    private LogModel() { }

    /// <summary>
    /// Initializes a new instance of <see cref="LogModel"/>
    /// </summary>
    public LogModel(string user, string action)
    {
        User = user;
        Action = action;
        LogTimestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// The user that the log comes from
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// The action being logged
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp for the Log
    /// </summary>
    public DateTime? LogTimestamp { get; set; }
}
