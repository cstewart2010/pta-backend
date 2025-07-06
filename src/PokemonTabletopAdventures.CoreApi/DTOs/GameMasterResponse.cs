using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.DTOs.Users;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class GameMasterResponse : AbstractDto
{
    internal static async Task<GameMasterResponse> ParseFromModel(
        UserModel user,
        Guid gameId,
        ITrainerService trainerService,
        IPokemonService pokemonService,
        IPokedexService pokedexService)
    {
        var message = "Game was found";
        var gameMasterId = user.UserId;
        var models = await trainerService.GetTrainersByGameId(gameId);
        //var trainers = await Task.WhenAll(models.Select(async trainer => await Trainer.ParseFromModel(trainer, pokemonService, pokedexService)));

        return new GameMasterResponse
        {
            User = new User(user),
            GameId = gameId,
            GameMasterId = gameMasterId,
            Message = message,
            Trainers = []
        };
    }

    public Guid GameId { get; init; }
    public Guid GameMasterId { get; init; }
    public required IEnumerable<Trainer> Trainers { get; init; }
    public required User User { get; init; }
}
