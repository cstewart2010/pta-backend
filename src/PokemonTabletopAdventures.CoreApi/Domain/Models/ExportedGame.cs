using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;

namespace PokemonTabletopAdventures.CoreApi.Domain.Models;

internal class ExportedGame
{
    public ExportedGame() { }

    public static async Task<ExportedGame> ParseFromModel(
        GameDto game,
        ITrainerService trainerService,
        IPokemonService pokemonService,
        IModelToDtoMapper modelToDtoMapper)
    {
        var trainers = await trainerService.GetTrainersByGameId(game.GameId);
        var gameSession = new GameDto
        {
            GameId = game.GameId,
            IsOnline = game.IsOnline,
            Logs = [],
            Nickname = game.Nickname,
            NPCs = game.NPCs,
            PasswordHash = game.PasswordHash
        };
        var exportedTrainers = await Task.WhenAll(trainers.Select(async trainer => await ExportedTrainer.ParseFromModel(await modelToDtoMapper.ParseFromModel(trainer), trainerService, pokemonService, modelToDtoMapper)));

        return new ExportedGame
        {
            GameSession = gameSession,
            Trainers = exportedTrainers
        };
    }

    public required GameDto GameSession { get; set; }
    public required IEnumerable<ExportedTrainer> Trainers { get; set; }
}
