using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class TrainerCollectionImpl : BaseCollectionImpl<TrainerDto>
{
    public override ICollection<TrainerDto> Collection { get; protected set; } = Shared.UserIds.Select((x, trainerIndex) => Shared.GameIds.Select((y, gameIndex) => new TrainerDto
    {
        Age = Random.Shared.Next(0, 100),
        Background = x.ToString(),
        CurrentHP = 20,
        Description = x.ToString(),
        Feats = [..Enumerable.Range(0, 3).Select(_ => x.ToString())],
        GameId = y,
        TrainerId = x,
        Gender = Gender.Male,
        Goals = x.ToString(),
        Height = 5,
        Honors = [..Enumerable.Range(0, 3).Select(_ => x.ToString())],
        IsAllowed = true,
        IsComplete = true,
        IsGM = trainerIndex == gameIndex,
        Weight = 100,
        IsOnline = true,
        Sprite = x.ToString(),
        Items = [],
        Money = 100,
        Personality = x.ToString(),
        Species = x.ToString(),
        TrainerClasses = [..Enumerable.Range(0, 3).Select(_ => x.ToString())],
        TrainerName = x.ToString(),
        TrainerSkills = [],
        TrainerStats = new Stats
        {
            HP = 20,
            Speed = 5,
        },
        Origin = x.ToString()
    })).SelectMany(x => x).ToList();
}