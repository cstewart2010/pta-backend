using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class CreatedGameResponse : AbstractDto
{
    internal static async Task<CreatedGameResponse> ParseFromModel(TrainerModel trainer, IPokemonService pokemonService, IPokedexService pokedexService)
    {
        var gameMaster = await Trainer.ParseFromModel(trainer, pokemonService, pokedexService);
        return new CreatedGameResponse
        {
            Message = "Game was created",
            GameId = trainer.GameId,
            GameMaster = gameMaster
        };
    }

    public required Guid GameId { get; init; }
    public required Trainer GameMaster { get; init; }
}
