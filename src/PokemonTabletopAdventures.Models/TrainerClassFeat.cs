namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents a <see cref="FeatureModel"/> learned as part of a <see cref="TrainerClassModel"/>
/// </summary>
public class TrainerClassFeat
{
    /// <summary>
    /// The naem of the feature
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The level that the feature is learned
    /// </summary>
    public int LevelLearned { get; set; }
}
