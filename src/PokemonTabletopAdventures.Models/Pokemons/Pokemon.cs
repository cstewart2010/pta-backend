using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models.Pokemons;

public class Pokemon
{
    public required Guid PokemonId { get; set; }
    public required int DexNo { get; set; }
    public required string Form { get; set; }
    public required IEnumerable<string> AlternateForms { get; set; }
    public required string NormalPortrait { get; set; }
    public required string ShinyPortrait { get; set; }
    public required string SpeciesName { get; set; }
    public required Guid TrainerId { get; set; }
    public required Guid GameId { get; set; }
    public required Guid OriginalTrainerId { get; set; }
    public required Gender Gender { get; set; }
    public required Status PokemonStatus { get; set; }
    public required string Nickname { get; set; }
    public required IEnumerable<string> Moves { get; set; }
    public required string Type { get; set; }
    public required Nature Nature { get; set; }
    public required bool IsShiny { get; set; }
    public required bool IsOnActiveTeam { get; set; }
    public required bool CanEvolve { get; set; }
    public required Stats PokemonStats { get; set; }
    public required string Pokeball { get; set; }
    public required int CurrentHP { get; set; }
    public required Size Size { get; set; }
    public required Weight Weight { get; set; }
    public required IEnumerable<string> Skills { get; set; }
    public required IEnumerable<string> Passives { get; set; }
    public required IEnumerable<string> EggGroups { get; set; }
    public required IEnumerable<string> Proficiencies { get; set; }
    public required string EggHatchRate { get; set; }
    public required IEnumerable<string> Habitats { get; set; }
    public required string Diet { get; set; }
    public required string Rarity { get; set; }
    public required string GMaxMove { get; set; }
    public required string EvolvedFrom { get; set; }
    public required LegendaryStats? LegendaryStats { get; set; }
    public int CatchRate { get; set; }
}
