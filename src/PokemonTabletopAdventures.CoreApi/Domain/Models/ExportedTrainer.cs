using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain.Models;

internal class ExportedTrainer
{
    public ExportedTrainer() { }

    public static async Task<ExportedTrainer> ParseFromModel(
        TrainerModel trainer,
        ITrainerService trainerService,
        IPokemonService pokemonService)
    {
        trainer.IsOnline = false;
        await trainerService.UpdateTrainerOnlineStatus(trainer.TrainerId, trainer.GameId, false);
        return new ExportedTrainer
        {
            Trainer = trainer,
            Pokemon = await pokemonService.GetPokemonByTrainerId(trainer.TrainerId, trainer.GameId)
        };
    }

    public required TrainerModel Trainer { get; set; }
    public required IEnumerable<PokemonModel> Pokemon { get; set; }
}
