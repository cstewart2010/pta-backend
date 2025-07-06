using PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Trainers;

public class Trainer
{
    internal Trainer() { }

    internal static async Task<Trainer> ParseFromModel(TrainerModel trainer, IPokemonService pokemonService, IPokedexService pokedexService)
    {
        var trainerPokemon = await pokemonService.GetPokemonByTrainerId(trainer.TrainerId, trainer.GameId);
        var pokedex = (await pokedexService.GetTrainerPokeDex(trainer.TrainerId, trainer.GameId)).OrderBy(item => item.DexNo);
        var caught = pokedex.Count(dexItem => dexItem.IsCaught);
        return new Trainer
        {
            TrainerId = trainer.TrainerId,
            TrainerName = trainer.TrainerName,
            IsGM = trainer.IsGM,
            IsOnline = trainer.IsOnline,
            Feats = trainer.Feats,
            GameId = trainer.GameId,
            Honors = trainer.Honors,
            Money = trainer.Money,
            Origin = trainer.Origin,
            TrainerClasses = trainer.TrainerClasses,
            TrainerStats = trainer.TrainerStats,
            IsComplete = trainer.IsComplete,
            PokemonTeam = trainerPokemon.Where(pokemon => pokemon.IsOnActiveTeam),
            PokemonHome = trainerPokemon.Where(pokemon => !pokemon.IsOnActiveTeam),
            PokeDex = pokedex.OrderBy(item => item.DexNo),
            SeenTotal = pokedex.Count(dexItem => dexItem.IsSeen),
            CaughtTotal = caught,
            Level = trainer.Honors.Count() + caught / 30 + 1,
            TrainerSkills = trainer.TrainerSkills,
            Age = trainer.Age,
            Gender = trainer.Gender,
            Height = trainer.Height,
            Weight = trainer.Weight,
            Description = trainer.Description,
            Personality = trainer.Personality,
            Background = trainer.Background,
            Goals = trainer.Goals,
            Species = trainer.Species,
            Items = trainer.Items,
            CurrentHP = trainer.CurrentHP,
            IsAllowed = trainer.IsAllowed,
            NewPokemon = [],
            Sprite = trainer.Sprite,
        };
    }

    internal async Task<TrainerModel> ParseBackToModel(ITrainerService trainerService)
    {
        var trainer = await trainerService.GetTrainerById(TrainerId, GameId);
        trainer.TrainerName = TrainerName;
        trainer.Feats = Feats;
        trainer.Money = Money;
        trainer.Origin = Origin;
        trainer.TrainerClasses = TrainerClasses;
        trainer.TrainerStats = TrainerStats;
        trainer.TrainerSkills = TrainerSkills;
        trainer.Age = Age;
        trainer.Gender = Gender;
        trainer.Height = Height;
        trainer.Weight = Weight;
        trainer.Description = Description;
        trainer.Personality = Personality;
        trainer.Background = Background;
        trainer.Goals = Goals;
        trainer.Items = [.. Items];
        trainer.Species = Species;
        trainer.CurrentHP = CurrentHP;
        trainer.Sprite = Sprite;
        if (!(IsComplete || string.IsNullOrEmpty(Origin)))
        {
            trainer.IsComplete = true;
        }
        return trainer;
    }

    public required Guid TrainerId { get; set; }
    public required string TrainerName { get; set; }
    public required bool IsGM { get; set; }
    public required bool IsOnline { get; set; }
    public required IEnumerable<string> Feats { get; set; }
    public required Guid GameId { get; set; }
    public required IEnumerable<string> Honors { get; set; }
    public required int Money { get; set; }
    public required string Origin { get; set; }
    public required IEnumerable<string> TrainerClasses { get; set; }
    public required IEnumerable<PokemonModel> PokemonTeam { get; set; }
    public required IEnumerable<PokemonModel> PokemonHome { get; set; }
    public required IEnumerable<PokeDexItemModel> PokeDex { get; set; }
    public required IEnumerable<NewPokemon> NewPokemon { get; set; }
    public required StatsModel TrainerStats { get; set; }
    public required bool IsComplete { get; set; }
    public required bool IsAllowed { get; set; }
    public required int SeenTotal { get; set; }
    public required int CaughtTotal { get; set; }
    public required int Level { get; set; }
    public required IEnumerable<ItemModel> Items { get; set; }
    public required IEnumerable<TrainerSkill> TrainerSkills { get; set; }
    public required int Age { get; set; }
    public required string Sprite { get; set; }
    public required string Gender { get; set; }
    public required int Height { get; set; }
    public required int Weight { get; set; }
    public required string Description { get; set; }
    public required string Personality { get; set; }
    public required string Background { get; set; }
    public required string Goals { get; set; }
    public required string Species { get; set; }
    public required int CurrentHP { get; set; }
}
