using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;

namespace PokemonTabletopAdventures.CoreApi.Domain.Models;

internal class ExportedTrainer
{
    public ExportedTrainer() { }

    public static async Task<ExportedTrainer> ParseFromModel(
        TrainerDto trainer,
        ITrainerService trainerService,
        IPokemonService pokemonService,
        IModelToDtoMapper modelToDtoMapper)
    {
        trainer.IsOnline = false;
        await trainerService.UpdateTrainerOnlineStatus(trainer.TrainerId, trainer.GameId, false);
        var pokemon = await pokemonService.GetPokemonByTrainerId(trainer.TrainerId, trainer.GameId, false);
        return new ExportedTrainer
        {
            Trainer = trainer,
            Pokemon = [.. await Task.WhenAll(pokemon.Select(modelToDtoMapper.ParseFromModel))],
        };
    }

    public required TrainerDto Trainer { get; set; }
    public required IEnumerable<PokemonDto> Pokemon { get; set; }
}
