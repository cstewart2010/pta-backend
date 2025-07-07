using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models.Settings;

/// <summary>
/// Represents a participant to an encounter during a PTA session
/// </summary>
public class SettingParticipant
{
    /// <summary>
    /// The paticipant's id
    /// </summary>
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// The paticipant's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The pariticipant's type (Trainer/Pokemon/Npc)
    /// </summary>
    public SettingParticipantType Type { get; set; }

    /// <summary>
    /// A description of the participant's health
    /// </summary>
    public string Health { get; set; } = string.Empty;

    /// <summary>
    /// The pariticipant's speed
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// The participants's position on the map
    /// </summary>
    public MapPosition Position { get; set; } = new();
}