using System.ComponentModel.DataAnnotations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokedex;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.Models.Trainers;

public class Trainer
{
    public required Guid TrainerId { get; set; }
    [Required, MinLength(6), MaxLength(20)]
    public required string TrainerName { get; set; }
    public required bool IsGM { get; set; }
    public required bool IsOnline { get; set; }
    public required IEnumerable<string> Feats { get; set; }
    public required Guid GameId { get; set; }
    public required IEnumerable<string> Honors { get; set; }
    public required int Money { get; set; }
    public required string Origin { get; set; }
    public required IEnumerable<string> TrainerClasses { get; set; }
    public required IEnumerable<Pokemon> PokemonTeam { get; set; }
    public required IEnumerable<Pokemon> PokemonHome { get; set; }
    public required IEnumerable<PokedexItem> PokeDex { get; set; }
    public required IEnumerable<NewPokemon> NewPokemon { get; set; }
    public required Stats TrainerStats { get; set; }
    public required bool IsComplete { get; set; }
    public required bool IsAllowed { get; set; }
    public required int SeenTotal { get; set; }
    public required int CaughtTotal { get; set; }
    public required int Level { get; set; }
    public required IEnumerable<Item> Items { get; set; }
    public required IEnumerable<TrainerSkill> TrainerSkills { get; set; }
    public required int Age { get; set; }
    public required string Sprite { get; set; }
    public required Gender Gender { get; set; }
    public required int Height { get; set; }
    public required int Weight { get; set; }
    public required string Description { get; set; }
    public required string Personality { get; set; }
    public required string Background { get; set; }
    public required string Goals { get; set; }
    public required string Species { get; set; }
    public required int CurrentHP { get; set; }
}
