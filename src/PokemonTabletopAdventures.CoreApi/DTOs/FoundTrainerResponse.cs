using PokemonTabletopAdventures.CoreApi.DTOs.Users;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class FoundTrainerResponse : AbstractDto
{
    internal static async Task<FoundTrainerResponse> ParseFromModel(
        TrainerModel model,
        UserModel user,
        IPokemonService pokemonService,
        IPokedexService pokedexService,
        IGameService gameService)
    {
        return new FoundTrainerResponse
        {
            Message = "Trainer was found",
            User = new User(user),
            GameNickname = await gameService.GetGameNickname(model.GameId),
        };
    }

    public required User User { get; init; }
    public required string GameNickname { get; init; }
}
