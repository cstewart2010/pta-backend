using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain.Models;

/// <summary>
/// Represents a Game Session for external use
/// </summary>
public class MinifiedGameModel
{
    internal static async Task<MinifiedGameModel> ParseFromModel(GameModel game, ITrainerService service)
    {
        var trainers = await service.GetTrainersByGameId(game.GameId);
        return new MinifiedGameModel
        {
            GameId = game.GameId,
            Nickname = game.Nickname,
            GameMasters = trainers
            .Where(x => x.IsGM)
            .Select(trainer => trainer.TrainerName)
        };
    }

    /// <summary>
    /// The PTA game session id
    /// </summary>
    public required Guid GameId { get; init;  }

    /// <summary>
    /// The PTA game masters
    /// </summary>
    public required IEnumerable<string> GameMasters{ get; init; }

    /// <summary>
    /// A user-friendly nickname for the game session
    /// </summary>
    public required string Nickname { get; init; }
}
