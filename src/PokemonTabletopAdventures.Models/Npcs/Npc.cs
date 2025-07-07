using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokemons;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Npcs;

public class Npc
{
    public required Guid NpcId { get; set; }

    public required Guid GameId { get; set; }

    [Required, MinLength(6), MaxLength(20)]
    public required string TrainerName { get; set; }

    [Required]
    public required IEnumerable<string> Feats { get; set; }

    [Required]
    public required IEnumerable<string> TrainerClasses { get; set; }

    public required Stats TrainerStats { get; set; }

    public required IEnumerable<Pokemon> PokemonTeam { get; set; }

    public required int Level { get; set; }

    public required IEnumerable<TrainerSkill> TrainerSkills { get; set; }

    public int Age { get; set; }

    public required Gender Gender { get; set; }

    public required int Height { get; set; }

    public required int Weight { get; set; }

    public required string Description { get; set; }

    public required string Personality { get; set; }

    public required string Background { get; set; }

    public required string Goals { get; set; }

    public required string Species { get; set; }

    public required string Sprite { get; set; }

    public int CurrentHP { get; set; }
}
