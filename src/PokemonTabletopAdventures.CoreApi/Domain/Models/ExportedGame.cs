using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain.Models;

internal class ExportedGame
{
    public ExportedGame() { }

    public static async Task<ExportedGame> ParseFromModel(
        GameModel game,
        ITrainerService trainerService,
        IPokemonService pokemonService)
    {
        var trainers = await trainerService.GetTrainersByGameId(game.GameId);
        var gameSession = new GameModel
        {
            GameId = game.GameId,
            IsOnline = game.IsOnline,
            Logs = [],
            Nickname = game.Nickname,
            NPCs = game.NPCs,
            PasswordHash = game.PasswordHash
        };
        var exportedTrainers = await Task.WhenAll(trainers.Select(async trainer => await ExportedTrainer.ParseFromModel(trainer, trainerService, pokemonService)));

        return new ExportedGame
        {
            GameSession = gameSession,
            Trainers = exportedTrainers
        };
    }

    public required GameModel GameSession { get; set; }
    public required IEnumerable<ExportedTrainer> Trainers { get; set; }
}
