using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Pokedex;

public class PokedexItem
{
    [Required]
    public required Guid TrainerId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required int DexNo { get; set; }
    [Required]
    public required bool IsSeen { get; set; }
    [Required]
    public required bool IsCaught { get; set; }
}
